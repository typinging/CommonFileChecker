using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    public interface IParser
    {
        CNode Root { get; }

        CNode CurrentNode { get; }

        bool Parser(string path);
    }

    public interface IEditor
    {
        bool EditFile(string path, FileNode node, string outputPath);
    }
}
