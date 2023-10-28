using System;
using System.ComponentModel;
using System.Resources;

namespace SyncIMEStatus.Helpers
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey)
        {
            this._resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = ResourceHelper.GetString(this._resourceKey);
                return string.IsNullOrWhiteSpace(description) ?
                    this._resourceKey : description;
            }
        }
    }
}
