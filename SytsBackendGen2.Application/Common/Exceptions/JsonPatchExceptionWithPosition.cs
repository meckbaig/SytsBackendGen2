using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Common.Exceptions;

public class JsonPatchExceptionWithPosition : JsonPatchException
{
    public int Position { get; set; }

    public JsonPatchExceptionWithPosition()
    {

    }

    public JsonPatchExceptionWithPosition(JsonPatchError jsonPatchError, Exception innerException, int position = 0)
        : base(jsonPatchError.ErrorMessage, innerException)
    {
        Position = position;
    }

    public JsonPatchExceptionWithPosition(JsonPatchError jsonPatchError, int position = 0)
      : this(jsonPatchError, null)
    {
        Position = position;
    }

    public JsonPatchExceptionWithPosition(string message, Exception innerException, int position = 0)
        : base(message, innerException)
    {
        Position = position;
    }
}
