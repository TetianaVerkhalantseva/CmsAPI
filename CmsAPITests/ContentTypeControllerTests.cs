using CmsAPI.Controllers;
using CmsAPI.Models;
using CmsAPI.Services.ContentTypeServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CmsAPITests
{
    public class ContentTypeControllerTests
    {
        private readonly Mock<IContentTypeService> _contentTypeServiceMock;
        private readonly ContentTypeController _controller;
        private readonly string[] _contentTypes = { "Word", "Image", "Video", "PDF", "Audio", "Link", "Contact", "Text" };

        public ContentTypeControllerTests()
        {
            _contentTypeServiceMock = new Mock<IContentTypeService>();
            _controller = new ContentTypeController(_contentTypeServiceMock.Object);
        }

        [Fact]
        public async Task GetContentTypes_ReturnsOk_WhenContentTypesExist()
        {
            // Arrange
            var contentTypes = _contentTypes.Select((type, index) => new ContentTypeDto
            {
                Id = index + 1,
                Name = type
            }).ToList();
            _contentTypeServiceMock.Setup(service => service.GetAll())
                .ReturnsAsync(contentTypes);

            // Act
            var result = await _controller.GetContentTypes() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(contentTypes, result.Value);
        }

        [Fact]
        public async Task GetContentTypes_ReturnsEmptyList_WhenNoContentTypesExist()
        {
            // Arrange
            _contentTypeServiceMock.Setup(service => service.GetAll())
                .ReturnsAsync(new List<ContentTypeDto>());

            // Act
            var result = await _controller.GetContentTypes() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var contentTypes = result.Value as IEnumerable<ContentTypeDto>;
            Assert.Empty(contentTypes);
        }
    }
}