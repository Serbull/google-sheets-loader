using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEditor;
using System.Threading.Tasks;

namespace GoogleSheets
{
    public class CVSLoader : MonoBehaviour
    {
        private const string _url = "https://docs.google.com/spreadsheets/d/{0}?format=csv&gid={1}";
        private const string _urlExport = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";

        public static string GetUrl(string sheetId, string pageId, bool export = false)
        {
            return string.Format(export ? _urlExport : _url, sheetId, pageId);
        }

        public static async void DownloadTable(string sheetId, string pageId, Action<string> loadSuccessAction, Action<string> loadErrorAction)
        {
            string actualUrl = string.Format(_urlExport, sheetId, pageId);

            using UnityWebRequest request = UnityWebRequest.Get(actualUrl);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(100);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                loadErrorAction?.Invoke(request.error);
            }
            else
            {
                loadSuccessAction?.Invoke(request.downloadHandler.text);
            }
        }

        public static string[,] SplitCsvGrid(string csvText)
        {
            string[] lines = csvText.Split("\n"[0]);

            // finds the max width of row
            int width = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] row = SplitCsvLine(lines[i]);
                width = Mathf.Max(width, row.Length);
            }

            // creates new 2D string grid to output to
            string[,] outputGrid = new string[width + 1, lines.Length + 1];
            for (int y = 0; y < lines.Length; y++)
            {
                string[] row = SplitCsvLine(lines[y]);
                for (int x = 0; x < row.Length; x++)
                {
                    outputGrid[x, y] = row[x];

                    // This line was to replace "" with " in my output. 
                    // Include or edit it as you wish.
                    outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
                }
            }

            return outputGrid;
        }

        private static string[] SplitCsvLine(string line)
        {
            return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
            @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                    select m.Groups[1].Value).ToArray();
        }
    }
}
