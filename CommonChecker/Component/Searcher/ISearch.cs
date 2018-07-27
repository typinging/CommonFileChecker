using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    internal interface ISearch
    {
        string CreateSearchString(FileNode node, string searchString);
        Dictionary<string, int> Search(string filePath, string searchPath);
    }
}
