namespace CommonChecker
{
    public class Editor
    {
        private IEditor _editor;

        public void Set_EditorInstance(IEditor edt)
        {
            this._editor = edt;
        }

        public bool ParseFile(string filePath, FileNode fNode, string output)
        {
            return this._editor.EditFile(filePath, fNode, output);
        }
    }
}
