using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUdttHelperTests
{
    public static class AssertExtensions
    {
        public static void Throws(Action func)
        {
            var exceptionThrown = false;
            string exTypeName = String.Empty;
            try
            {
                func.Invoke();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                exTypeName = ex.GetType().Name;
            }

            if (!exceptionThrown)
            {
                throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                    String.Format("An exception of type {0} was expected, but not thrown", exTypeName)
                    );
            }
        }

        public static void Throws<T>(Action func) where T : Exception
        {
            var exceptionThrown = false;
            try
            {
                func.Invoke();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                    String.Format("An exception of type {0} was expected, but not thrown", typeof(T))
                    );
            }
        }

        public static void DoesNotThrow(Action func)
        {
            var exceptionThrown = false;
            string exTypeName = String.Empty;
            try
            {
                func.Invoke();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                exTypeName = ex.GetType().Name;
            }

            if (!exceptionThrown)
            {
                throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                    String.Format("An exception of type {0} was expected, but not thrown", exTypeName)
                    );
            }
        }

        public static void DoesNotThrow<T>(Action func) where T : Exception
        {
            var exceptionThrown = false;
            try
            {
                func.Invoke();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            if (exceptionThrown)
            {
                throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                    String.Format("An exception of type {0} was unexpectedly thrown", typeof(T))
                    );
            }
        }
    }
}
