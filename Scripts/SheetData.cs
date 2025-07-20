using UnityEngine;
using GoogleSheets.Values;

namespace GoogleSheets
{
    public abstract class SheetData : ScriptableObject
    {
        [SerializeField] private string _sheetId;
        [SerializeField] private string _pageId = "0";
        [SerializeField] private string _lastFetchTime;
        [SerializeField] private bool _lastFetchSuccessful;

        protected string[,] _cells;

        protected SheetDataValue Base;
        protected SheetDataInt Int;
        protected SheetDataFloat Float;
        protected SheetDataString String;

        public void DownloadTable()
        {
            Debug.Log($"Start download table for '{name}'");
            CVSLoader.DownloadTable(_sheetId, _pageId, OnDownloadSuccess, OnDownloadError);
        }

        private void OnDownloadSuccess(string data)
        {
            Debug.Log($"Download successful for '{name}'");
            _lastFetchTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

            try
            {
                UpdateData(data);
                _lastFetchSuccessful = true;
            }
            catch (System.Exception exception)
            {
                Debug.LogError(exception);
                _lastFetchSuccessful = false;
                HandleException(exception);
            }
        }

        private void OnDownloadError(string error)
        {
            Debug.LogError($"Download error: {error}");
        }

        public void UpdateData(string data)
        {
            _cells = CVSLoader.SplitCsvGrid(data);

            Base = new SheetDataValue(_cells);
            Int = new SheetDataInt(_cells);
            Float = new SheetDataFloat(_cells);
            String = new SheetDataString(_cells);

            UpdateSheetData();
        }

        protected abstract void UpdateSheetData();

        protected virtual void HandleException(System.Exception ex) { }
    }
}
