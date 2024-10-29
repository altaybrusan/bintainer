using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Repository.Interface;
using Bintainer.Service;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Bintainer.Test.Service
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<IOrderRepository> _mockOrderRepository;
        private Mock<IPartRepository> _mockPartRepository;
        private Mock<IMapper> _mockMapper;
        private OrderService _orderService;

        [SetUp]
        public void Setup()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockPartRepository = new Mock<IPartRepository>();
            _mockMapper = new Mock<IMapper>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockPartRepository.Object, _mockMapper.Object);
        }

        [Test]
        public void RegisterOrder_WhenOrderDoesNotExist_ShouldRegisterNewOrder()
        {
            // Arrange
            var request = new RegisterOrderRequest
            {
                OrderNumber = "ORD123",
                OrderDate = new DateTime(2023, 1, 1),
                HandoverDate = new DateTime(2023, 1, 10),
                Supplier = "Supplier1",
                Parts = new List<OrderItem>
                {
                    new("part" , 5,  1) 
                }
            };
            var userId = "user1";
            _mockOrderRepository.Setup(r => r.GetOrderByOrderNumber(request.OrderNumber, userId)).Returns((Order)null);
            _mockPartRepository.Setup(r => r.GetPartById(1)).Returns(new Part { Id = 1, Name = "Part1" });
            _mockOrderRepository.Setup(r => r.AddAndSaveOrder(It.IsAny<Order>()));

            // Act
            var result = _orderService.RegisterOrder(request, userId);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Result);
            Assert.That(result.Result.OrderNumber, Is.EqualTo("ORD123"));
            _mockOrderRepository.Verify(r => r.AddAndSaveOrder(It.IsAny<Order>()), Times.Once);
        }

        [Test]
        public void RegisterOrder_WhenOrderExists_ShouldUpdateOrder()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                OrderNumber = "ORD123",
                OrderDate = new DateTime(2023, 1, 1),
                HandOverDate = new DateTime(2023, 1, 10),
                Supplier = "Supplier1",
                OrderPartAssociations = new List<OrderPartAssociation>
                {
                    new OrderPartAssociation { PartId = 10, Quantity = 3 }
                }
            };
            var request = new RegisterOrderRequest
            {
                OrderNumber = "ORD123",
                OrderDate = new DateTime(2023, 1, 2),
                HandoverDate = new DateTime(2023, 1, 12),
                Supplier = "Updated Supplier",
                Parts = new List<OrderItem>
                {
                    new("Part1",1,10)
                }
            };
            var userId = "user1";
            _mockOrderRepository.Setup(r => r.GetOrderByOrderNumber(request.OrderNumber, userId)).Returns(existingOrder);
            _mockOrderRepository.Setup(r => r.UpdateOrder(existingOrder));

            // Act
            var result = _orderService.RegisterOrder(request, userId);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.That(existingOrder.Supplier, Is.EqualTo("Updated Supplier"));
            Assert.That(existingOrder.OrderPartAssociations.First().Quantity, Is.EqualTo(1));
            _mockOrderRepository.Verify(r => r.UpdateOrder(existingOrder), Times.Once);
        }

        [Test]
        public void FilterOrder_WhenOrdersExist_ShouldReturnFilteredOrders()
        {
            // Arrange
            var request = new FilterOrderRequest()
            {
                OrderNumber = "ORD123"
            };
            var requestModel = new FilterOrderRequestModel() 
            {
                OrderNumber = "ORD123"
            };
            _mockMapper.Setup(m => m.Map<FilterOrderRequestModel>(request)).Returns(requestModel);

            var orders = new List<OrderInfo>
            {
                new OrderInfo
                {
                    OrderNumber = "ORD123",
                    Supplier = "Supplier1",
                    OrderDate = new DateTime(2023, 1, 1),
                    HandOverDate = new DateTime(2023, 1, 10),
                    Parts = new List<PartSummary>
                    {
                        new PartSummary { PartName = "Part1", Quantity = 5 }
                    }
                }
            };

            _mockOrderRepository.Setup(r => r.FilterOrder(requestModel)).Returns(orders);

            // Act
            var result = _orderService.FilterOrder(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Result);
            Assert.That(result.Result.Count, Is.EqualTo(1));
            Assert.That(result.Result.First().OrderNumber, Is.EqualTo("ORD123"));
            Assert.That(actual: result.Result.First().Parts!.First()?.PartName, Is.EqualTo("Part1"));
        }

        [Test]
        public void FilterOrder_WhenNoOrdersFound_ShouldReturnEmptyList()
        {
            // Arrange
            var request = new FilterOrderRequest();
            var requestModel = new FilterOrderRequestModel();
            _mockMapper.Setup(m => m.Map<FilterOrderRequestModel>(request)).Returns(requestModel);

            _mockOrderRepository.Setup(r => r.FilterOrder(requestModel)).Returns((List<OrderInfo>)null);

            // Act
            var result = _orderService.FilterOrder(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Result);
        }

    }

}