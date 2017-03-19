using Caliburn.Micro;
using ScoreboardFadeCandy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
    class StripController : PropertyChangedBase, IStripController, IDisposable
    {
        private ScoreboardFadeCandyController _fadeCandy;

        private ScoreboardFadeCandyController FadeCandy
        {
            get { return _fadeCandy; }
            set
            {
                _fadeCandy = value;
                if (_fadeCandy == null)
                {
                    _groupName = null;
                }
                else
                {
                    _groupName = _fadeCandy.LedNumbers.GroupNumbers.First().Key;
                }
            }
        }

        // This is the group used for 'strings' and inc/dec
        private string _groupName;

        public bool IsInitialised => FadeCandy != null;

        public StripController()
        {
        }

        public void Initialise()
        {
            FadeCandy = new ScoreboardFadeCandyController();
            try
            {
                FadeCandy.Initialise();
            }
            catch
            {
                FadeCandy.Dispose();
                FadeCandy = null;
                throw;
            }

            NotifyOfPropertyChange(() => IsInitialised);
        }

        public async Task ExecuteTestAsync(bool execute)
        {
            if (FadeCandy != null)
                await FadeCandy.ExecuteTestAsync(execute);
        }

        public async Task ExecuteNumberTestAsync(bool execute)
        {
            if (FadeCandy != null)
                await FadeCandy.ExecuteNumberTestAsync(execute);
        }

        public void SetStringValue(string value)
        {
            FadeCandy.LedNumbers.SetStringValue(_groupName, value);
            NotifyOfPropertyChange("Value");
        }

        public string GetStringValue()
        {
            return FadeCandy?.LedNumbers.GetStringValue(_groupName);
        }

        public void Inc()
        {
            uint? value = FadeCandy.LedNumbers.GetValue(_groupName);
            if (value == null)
            {
                value = 1;
            }
            else
            {
                value++;
            }

            try
            {
                SetStringValue(value.ToString());
            }
            catch
            {
                // assume out of range
            }
        }

        public void Dec()
        {
            uint? value = FadeCandy.LedNumbers.GetValue(_groupName);
            if (value == 0)
            {
                value = null;
            }
            else if (value != null)
            {
                value--;
            }

            try
            {
                SetStringValue(value.ToString());
            }
            catch
            {
                // assume out of range
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (FadeCandy != null)
                    {
                        FadeCandy.Dispose();
                        FadeCandy = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StripController() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
