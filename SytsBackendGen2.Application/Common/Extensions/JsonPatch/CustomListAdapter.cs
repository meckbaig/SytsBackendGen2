using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.JsonPatch.Operations;
using SytsBackendGen2.Domain.Common;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.JsonPatch;

public class CustomListAdapter : ListAdapter
{
    private ResourceManager rm = new ResourceManager(typeof(CustomListAdapter));

    protected override bool TryGetPositionInfo(
        IList list,
        string segment,
        OperationType operationType,
        out PositionInfo positionInfo,
        out string errorMessage)
    {
        if (segment == "-")
        {
            positionInfo = new PositionInfo(PositionType.EndOfList, -1);
            errorMessage = null;
            return true;
        }

        if (int.TryParse(segment, out var entityId))
        {
            var entities = list.Cast<IEntityWithId>().ToList();
            if (entities == null)
            {
                positionInfo = new PositionInfo(PositionType.Invalid, -1);
                errorMessage = AdapterError.FormatInvalidListType();
                return false;
            }
            int entityPosition = entities.IndexOf(entities.FirstOrDefault(e => e.Id == entityId));
            if (entityPosition >= 0)
            {
                positionInfo = new PositionInfo(PositionType.Index, entityPosition);
                errorMessage = null;
                return true;
            }
            // As per JSON Patch spec, for Add operation the index value representing the number of elements is valid,
            // where as for other operations like Remove, Replace, Move and Copy the target index MUST exist.
            else if (operationType == OperationType.Add)
            {
                positionInfo = new PositionInfo(PositionType.EndOfList, -1);
                errorMessage = null;
                return true;
            }
            else
            {
                positionInfo = new PositionInfo(PositionType.OutOfBounds, -1);
                errorMessage = AdapterError.FormatIndexOutOfBounds(segment);
                return false;
            }
        }
        else
        {
            positionInfo = new PositionInfo(PositionType.Invalid, -1);
            errorMessage = AdapterError.FormatInvalidIndexValue(segment);
            return false;
        }
    }

    public override bool TryTraverse(
        object target, 
        string segment, 
        IContractResolver contractResolver, 
        out object value, 
        out string errorMessage)
    {
        var entities = (target as IList).Cast<IEntityWithId>().ToList();
        if (entities == null)
        {
            value = null;
            errorMessage = null;
            return false;
        }

        if (!int.TryParse(segment, out var entityId))
        {
            value = null;
            errorMessage = AdapterError.FormatInvalidIndexValue(segment);
            return false;
        }

        int entityPosition = entities.IndexOf(entities.FirstOrDefault(e => e.Id == entityId));
        if (entityPosition < 0)
        {
            value = null;
            errorMessage = AdapterError.FormatIndexOutOfBounds(segment);
            return false;
        }

        value = entities[entityPosition];
        errorMessage = null;
        return true;
    }

}
