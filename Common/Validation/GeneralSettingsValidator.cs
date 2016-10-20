using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Log;
using PhoneNumbers;

namespace Common.Validation
{
    public static class GeneralSettingsValidator
    {
        public static void Validate<T>(T settings, ILog log = null)
        {
            try
            {
                ValidationHelper.ValidateObjectRecursive(settings);
            }
            catch (Exception e)
            {
                log?.WriteFatalError("GeneralSettings", "Validation", null, e);

                throw;
            }
        }
    }
}
