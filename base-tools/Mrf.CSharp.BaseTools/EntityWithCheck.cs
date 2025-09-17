using System.ComponentModel;


namespace Mrf.CSharp.BaseTools
{

    /// <summary>
    /// 专门用来wpf中表格绑定 第一列为checkbox
    /// </summary>
    public class EntityWithCheck<T> : INotifyPropertyChanged
        {
            private T _entityInstance;

            public T EntityInstance
            {
                get { return _entityInstance; }
                set
                {
                    if (_entityInstance.Equals(value))
                    {
                        _entityInstance = value;
                        RaisePropertyChanged("EntityInstance");
                    }
                }
            }

            private bool _isChecked;

            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    if (_isChecked != value)
                    {
                        _isChecked = value;
                        RaisePropertyChanged("IsChecked");
                    }
                }
            }


            public event PropertyChangedEventHandler PropertyChanged;

            public void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

    }
