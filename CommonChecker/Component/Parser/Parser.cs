namespace CommonChecker
{
    public class Parser
    {
        private IParser _parser;

        public void Set_ParserInstance(IParser pI)
        {
            this._parser = pI;
        }

        public bool ParseFile(string fileName)
        {
            return this._parser.Parser(fileName);
        }

        public CNode Get_FileScheme()
        {
            return this._parser.Root;
        }

        public CNode GetCurrentNode()
        {
            return this._parser.CurrentNode;
        }
    }

    //public class Editor
    //{
    //    private IEditor _editor;

    //    public void Set_EditorInstance(IEditor edt)
    //    {
    //        this._editor = edt;
    //    }

    //    public bool ParseFile(string filePath, FileNode fNode, string output)
    //    {
    //        return this._editor.EditFile(filePath, fNode, output);
    //    }
    //}

}
