using UnityEngine;
using System.Windows.Forms;
using UnityEditor;

namespace CustomExporter
{
    public static class DialogManager
    {
        public const string KEY_PATH = "SavePath";

        public static string SAVE_FOLDER;

        [UnityEditor.MenuItem("Tools/Set Json Export Path")]
        public static void saveFileDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                EditorPrefs.SetString(KEY_PATH, dialog.SelectedPath);
                SAVE_FOLDER = dialog.SelectedPath;

                Debug.Log("New path found : " + dialog.SelectedPath);
            }
        }
    }
}
