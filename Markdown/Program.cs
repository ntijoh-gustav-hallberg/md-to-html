using System.Formats.Asn1;
using System.IO;
using System.IO.Enumeration;
using System.Text;
using System.Text.RegularExpressions;

string mdFolderPath = "./docs";
string htmlFolderPath = "./html/";

void getFiles() {
    // Read all files in directory
    foreach(string item in Directory.GetFiles(mdFolderPath, "*", SearchOption.AllDirectories)) {
        createFiles(getFileName(item));
        
        // Read file into array line by line
        string contents = File.ReadAllText(item);

        // TODO: img folder does not get created before causing error
        if(getFileName(item).Substring(getFileName(item).Length-4) == ".jpg") {
            Console.WriteLine(getFilePath(getFileName(item)));
            if(!File.Exists(getFilePath(getFileName(item)))) {
                File.Copy(item, getFilePath(getFileName(item)));
            }

            continue;
        }

        writeToFile(getFileName(item), contents);
    }
}

string getFileName(string filePath) {
    return filePath.Replace("./docs/", "");
}

void createFiles(string fileName) {
    string filePath = getFilePath(fileName);
    string directoryPath = htmlFolderPath + Path.GetDirectoryName(fileName);

    if(fileName.Substring(fileName.Length-4) == ".jpg") {
        return;
    }

    // Checks if directory exists, Creates if doesnt
    if(!Directory.Exists(filePath)) {
        Directory.CreateDirectory(directoryPath);
    }

    // Checks if file exist, delete if does
    if(File.Exists(filePath)) {
        File.Delete(filePath);
    }

    // Creates new files
    File.Create(filePath).Close();
}

void writeToFile(string fileName, string fileContent) {
    string filePath = getFilePath(fileName);

    string parsedFileContent = fileContent;

    if(fileName.Substring(fileName.Length-3) == ".md") {
        parsedFileContent = ParseMarkdown(fileContent);
    }

    using (StreamWriter outputFile = new StreamWriter(filePath))
    {
        outputFile.Write(parsedFileContent);
    }
}

string getFilePath(string fileName) {
    if(fileName.Substring(fileName.Length - 3) == ".md") {
        return htmlFolderPath + fileName.Substring(0, fileName.Length - 3) + ".html";
    }
    else {
        return htmlFolderPath + fileName;
    }
}

// Parse data
static string ParseMarkdown(string markdown)
{
    StringBuilder html = new StringBuilder();

    // Split input by lines
    string[] lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    int lineIndex = 0;
    bool hasStartedFrontMatter = false;

    foreach (var line in lines)
    {
        if(lineIndex <= 5) {
            string frontMatter = parseFrontMatter(line, hasStartedFrontMatter);
            if(frontMatter != "")
                html.AppendLine(frontMatter);

            lineIndex++;
            continue;
        }

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

static string parseFrontMatter(string line, bool hasStartedFrontMatter) {
    string title;
    string image;
    string posted;
    
    if(line.StartsWith("---")) {
        hasStartedFrontMatter = true;
        return "<head>";
    } else if (line.StartsWith("---") && hasStartedFrontMatter) {
        return "</head>";
    } else if(line.StartsWith("title:")) {
        title = line.Split(new[] {"title: "}, StringSplitOptions.None)[1];
        return $"<title>{title}</title>";
    } else if(line.StartsWith("image:")) {
        image = line.Split(new[] {"image: "}, StringSplitOptions.None)[1];
        return $"<meta type='image'>{image}</meta>";
    } else if(line.StartsWith("posted:")) {
        posted = line.Split(new[] {"posted: "}, StringSplitOptions.None)[1];
        return $"<meta type='posted'>{posted}</meta>";
    }

    return "";
}

getFiles();