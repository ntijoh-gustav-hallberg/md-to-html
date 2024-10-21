using System.IO;
using System.IO.Enumeration;
using System.Text;
using System.Text.RegularExpressions;

string mdFolderPath = "./docs";
string htmlFolderPath = "./html/";

void getFiles() {
    // Read all files in directory
    foreach(string file in Directory.EnumerateFiles(mdFolderPath, "*.md")) {
        // Read file into array line by line
        string contents = File.ReadAllText(file);

        createFiles(getFileName(file));

        writeToFile(getFileName(file), contents);
    }
}

string getFileName(string filePath) {
    return Path.GetFileNameWithoutExtension(filePath);
}

void createFiles(string fileName) {
    string filePath = htmlFolderPath + fileName + ".html";
    if(File.Exists(filePath)) {
        File.Delete(filePath);
    }

    File.Create(filePath).Close();
}

void writeToFile(string fileName, string fileContent) {
    string filePath = htmlFolderPath + fileName + ".html";
    string parsedFileContent = ParseMarkdown(fileContent);

    using (StreamWriter outputFile = new StreamWriter(filePath))
    {
        outputFile.Write(parsedFileContent);
    }
}

// Parse data
static string ParseMarkdown(string markdown)
{
    StringBuilder html = new StringBuilder();

    // Split input by lines
    string[] lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    foreach (var line in lines)
    {
        string htmlLine = ParseLine(line);
        html.AppendLine(htmlLine);
    }

    return html.ToString();
}

// Method to parse each line of the markdown
static string ParseLine(string line)
{
    // Parse headings (e.g. # H1, ## H2, ### H3)
    if (line.StartsWith("#"))
    {
        int headingLevel = line.TakeWhile(c => c == '#').Count();  // Count number of #
        string headingText = line.TrimStart('#').Trim();           // Remove # and trim spaces
        return $"<h{headingLevel}>{headingText}</h{headingLevel}>";
    }

    // Parse unordered list (e.g. - item)
    if (line.StartsWith("- "))
    {
        string listItem = line.Substring(2).Trim();
        return $"<ul><li>{listItem}</li></ul>";
    }

    // Parse bold (**bold**)
    line = Regex.Replace(line, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

    // Parse italic (*italic*)
    line = Regex.Replace(line, @"\*(.*?)\*", "<em>$1</em>");

    // Parse inline code (`code`)
    line = Regex.Replace(line, @"`(.*?)`", "<code>$1</code>");

    // Default: treat it as a paragraph
    if (!string.IsNullOrWhiteSpace(line))
    {
        return $"<p>{line}</p>";
    }

    return string.Empty;
}

void parseFrontMatter(string data) {
    // Split input by lines
    string[] lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    string title;
    string image;
    string posted;
    bool startLinePassed = false;

    foreach (var line in lines)
    {
        // Check if we are at start of frontmatter
        if(line == "---" && !startLinePassed) {
            startLinePassed = true;
            continue;
        }

        // Check if we are at end of frontmatter, if so break loop
        if(line == "---" && startLinePassed) {
            break;
        }
        
        if(line.StartsWith("title:")) {
            title = line.Split(new[] {"title: "}, StringSplitOptions.None)[1];
        } else if(line.StartsWith("image:")) {
            image = line.Split(new[] {"image: "}, StringSplitOptions.None)[1];
        } else if(line.StartsWith("posted:")) {
            posted = line.Split(new[] {"posted: "}, StringSplitOptions.None)[1];
        }
    }
}

getFiles();