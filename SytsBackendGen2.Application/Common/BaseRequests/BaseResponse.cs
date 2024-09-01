using System.Reflection;
namespace SytsBackendGen2.Application.Common.BaseRequests
{
    public abstract class BaseResponse
    {
        private Exception? _exception = null;
        public string? Message { private get; set; } = null;
        public string? GetMessage() => Message;
        public void SetException(Exception? exception) { _exception = exception; }
        public Exception? GetException() => _exception;

        public object this[string propertyName]
        {
            get
            {
                System.Type myType = GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                System.Type myType = GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
    }
}
