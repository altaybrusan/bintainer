using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;
using Bintainer.Model;
using Bintainer.Repository.Service;
using Bintainer.Model.Entity;
using Bintainer.Model.DTO;
using Bintainer.Model.Request;
using Moq;
using MockQueryable.Moq;
using NuGet.Protocol.Core.Types;
using Bintainer.Repository;


namespace Bintainer.Test.Repository
{

    [TestFixture]
    public class OrderRepositoryTests
    {
        private Mock<BintainerDbContext> _mockDbContext;
        private OrderRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<BintainerDbContext>();
            _repository = new OrderRepository(_mockDbContext.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            // Additional setup to enable querying on DbSet
            //mockSet.Setup(m => m.ToList()).Returns(data);

            return mockSet;
        }


        [Test]
        public void GetOrderByOrderNumber_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var orderNumber = "ORD123";
            var userId = "user123";
            var orders = new List<Order>
        {
            new Order { Id = 1, OrderNumber = orderNumber, UserId = userId, OrderPartAssociations = new List<OrderPartAssociation>() }
        };

            var mockSet = CreateMockDbSet(orders);
            _mockDbContext.Setup(c => c.Orders).Returns(mockSet.Object);

            // Act
            var result = _repository.GetOrderByOrderNumber(orderNumber, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderNumber, Is.EqualTo(orderNumber));
            Assert.That(result.UserId, Is.EqualTo(userId));
        }

        [Test]
        public void AddAndSaveOrder_AddsOrder_WhenOrderIsNotNull()
        {
            // Arrange
            var order = new Order { OrderNumber = "ORD123", UserId = "user123" };
            var mockSet = new Mock<DbSet<Order>>();

            _mockDbContext.Setup(c => c.Orders).Returns(mockSet.Object);

            // Act
            var result = _repository.AddAndSaveOrder(order);

            // Assert
            mockSet.Verify(m => m.Add(It.Is<Order>(o => o == order)), Times.Once);
            _mockDbContext.Verify(m => m.SaveChanges(true), Times.Once);
            Assert.That(result, Is.EqualTo(order));
        }

        [Test]
        public void UpdateOrder_UpdatesOrder_WhenOrderIsNotNull()
        {
            // Arrange
            var order = new Order { OrderNumber = "ORD123", UserId = "user123" };
            var mockSet = new Mock<DbSet<Order>>();

            _mockDbContext.Setup(c => c.Orders).Returns(mockSet.Object);

            // Act
            var result = _repository.UpdateOrder(order);

            // Assert
            mockSet.Verify(m => m.Update(It.Is<Order>(o => o == order)), Times.Once);
            _mockDbContext.Verify(m => m.SaveChanges(true), Times.Once);
            Assert.That(result, Is.EqualTo(order));
        }

        [Test]
        public void FilterOrder_ReturnsFilteredOrders_BasedOnRequest()
        {
            // Arrange
            var parts = new List<Part>
            {
                new Part { Id = 1, Name = "Part1" },
                new Part { Id = 2, Name = "Part2" }
            };

            var orderPartAssociations = new List<OrderPartAssociation>
            {
                new OrderPartAssociation { OrderId = 1, PartId = 1, Quantity = 10, Part= parts[0]},
                new OrderPartAssociation { OrderId = 1, PartId = 2, Quantity = 5 ,Part= parts[1]},
                new OrderPartAssociation { OrderId = 2, PartId = 1, Quantity = 15 ,Part= parts[0]}
            };


            var orders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD123", Supplier = "Supplier1", OrderDate = new DateTime(2023, 1, 1), UserId = "user1" },
                new Order { Id = 2, OrderNumber = "ORD124", Supplier = "Supplier2", OrderDate = new DateTime(2023, 2, 1), UserId = "user2" }
            };

            var mockOrderSet = orders.AsQueryable().BuildMock();
            var mockPartSet = parts.AsQueryable().BuildMock();
            var mockOrderPartSet = orderPartAssociations.AsQueryable().BuildMock();

            // Setup the mock DbContext
            _mockDbContext.Setup(c => c.Orders).Returns(mockOrderSet.BuildMockDbSet<Order>().Object);
            _mockDbContext.Setup(c => c.OrderPartAssociations).Returns(mockOrderPartSet.BuildMockDbSet<OrderPartAssociation>().Object);
            _mockDbContext.Setup(c => c.Parts).Returns(mockPartSet.BuildMockDbSet<Part>().Object); // Assuming you have a Parts DbSet

            var request = new FilterOrderRequestModel
            {
                OrderNumber = "ORD12",
                Supplier = "Supplier",
                FromDate = new DateTime(2023, 1, 1),
                ToDate = new DateTime(2023, 12, 31)
            };

            // Act
            var result = _repository.FilterOrder(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(r => r.OrderNumber == "ORD123"), Is.True);
            Assert.That(result.Any(r => r.OrderNumber == "ORD124"), Is.True);
        }



    }

}
