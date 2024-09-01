using Microsoft.AspNetCore.JsonPatch.Operations;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.JsonPatch;

internal class JsonPatchPath
{
    public readonly string OriginalPath;
    public readonly string Index;
    public readonly string AsSingleProperty;
    public readonly string? LastSegment;

    public JsonPatchPath(string path)
    {
        OriginalPath = path;

        string operationPathAsProperty = path.ToPropetyFormat();
        string[] pathSegments = operationPathAsProperty.Split('.');
        string index = pathSegments[0];
        if (int.TryParse(pathSegments[0], out int _) ||
            index == "-")
        {
            if (index.Length < operationPathAsProperty.Length)
                operationPathAsProperty = operationPathAsProperty[(index.Length + 1)..];
            else
                operationPathAsProperty = string.Empty;
        }
        else
        {
            index = string.Empty;
        }
        AsSingleProperty = operationPathAsProperty;
        Index = index;
        LastSegment = pathSegments.LastOrDefault();
    }

    public string ToFullPropertyPath(string newPropertyPath)
    {
        if (Index != string.Empty)
        {
            if (newPropertyPath.Length > 0)
                newPropertyPath = $"{Index}.{newPropertyPath}";
            else
                newPropertyPath = Index;
        }
        return newPropertyPath;
    }
}
