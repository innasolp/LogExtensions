namespace Serilog.Loggers.Test;

internal static class Common
{
    public static void AssertFileContent(string filePath, params string[] subStrings)
    {
var copyFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", $"{Path.GetFileNameWithoutExtension(filePath)}_copy{Path.GetExtension(filePath)}");
        File.Copy(filePath, copyFilePath);

        try
        {
            var content = File.ReadAllText(copyFilePath);
            foreach (var subString in subStrings)
                Assert.Contains(subString, content, StringComparison.InvariantCultureIgnoreCase);
        }
        finally
        {
            File.Delete(copyFilePath);
        }
    }

    public static void AssertFileContentNotContains(string filePath, params string[] subStrings)
    {
var copyFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", $"{Path.GetFileNameWithoutExtension(filePath)}_copy{Path.GetExtension(filePath)}");
File.Copy(filePath, copyFilePath, true);

        try
        {
            var content = File.ReadAllText(copyFilePath);
            foreach (var subString in subStrings)
                Assert.DoesNotContain(subString, content, StringComparison.InvariantCultureIgnoreCase);
        }
        finally
        {
            File.Delete(copyFilePath);
        }
    }
}