using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Tests;

internal static class Helper
{

    internal static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> queryable) where T : class
    {
        var dbSetMock = new Mock<DbSet<T>>();
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return dbSetMock;
    }
}
