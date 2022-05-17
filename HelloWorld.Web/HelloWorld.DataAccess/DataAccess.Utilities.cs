using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld;

public partial class DataAccess
{
    private string MaxStringLength(string? value, int maxLength)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(value))
        {
            output = value;
            if (output.Length > maxLength)
            {
                output = output.Substring(0, maxLength);
            }
        }

        return output;
    }
}
