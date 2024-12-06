using CmsAPI.Controllers;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.DocumentServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CmsAPITests
{
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentService> _documentServiceMock;
        private readonly DocumentController _controller;

        public DocumentControllerTests()
        {
            _documentServiceMock = new Mock<IDocumentService>();
            _controller = new DocumentController(_documentServiceMock.Object);
        }

        // GET /api/Document/user-documents
        [Fact]
        public async Task GetUserDocuments_ReturnsOk_WhenDocumentsExist()
        {
            // Arrange
            var documents = new List<DocumentDto> { new DocumentDto { DocumentId = 1, Title = "Test Document" } };
            _documentServiceMock.Setup(service => service.GetDocumentsByUserId())
                .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetUserDocuments() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documents, result.Value);
        }

        [Fact]
        public async Task GetUserDocuments_ReturnsNotFound_WhenNoDocumentsExist()
        {
            // Arrange
            _documentServiceMock.Setup(service => service.GetDocumentsByUserId())
                .ReturnsAsync(new List<DocumentDto>());

            // Act
            var result = await _controller.GetUserDocuments();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No documents found for the user.", ((NotFoundObjectResult)result).Value);
        }

        // GET /api/Document/{id}
        [Fact]
        public async Task GetDocumentById_ReturnsOk_WhenDocumentExists()
        {
            // Arrange
            var document = new DocumentDto { DocumentId = 1, Title = "Test Document" };
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync(document);

            // Act
            var result = await _controller.GetDocumentById(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document, result.Value);
        }

        [Fact]
        public async Task GetDocumentById_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync((DocumentDto)null);

            // Act
            var result = await _controller.GetDocumentById(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No document found for the document Id 1.", ((NotFoundObjectResult)result).Value);
        }

        // POST /api/Document
        [Fact]
        public async Task CreateDocument_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var invalidDto = new EditDocumentDto();
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            var result = await _controller.CreateDocument(invalidDto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.IsType<SerializableError>(result.Value);
            var error = result.Value as SerializableError;
            Assert.Contains("Title", error.Keys);
        }

        [Fact]
        public async Task CreateDocument_ReturnsCreatedAtAction_WhenDocumentIsValid()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "New Document" };
            var createdDocument = new Document { DocumentId = 1, Title = "New Document" };
            _documentServiceMock.Setup(service => service.GetDocumentByTitle(editDocumentDto.Title))
                .ReturnsAsync((Document)null);
            _documentServiceMock.Setup(service => service.CreateDocument(editDocumentDto))
                .ReturnsAsync(createdDocument);

            // Act
            var result = await _controller.CreateDocument(editDocumentDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GetDocumentById", result.ActionName);
            Assert.Equal(createdDocument, result.Value);
        }

        [Fact]
        public async Task CreateDocument_ReturnsConflict_WhenDocumentWithSameTitleExists()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "Existing Document" };
            var existingDocument = new Document { DocumentId = 1, Title = "Existing Document" };
            _documentServiceMock.Setup(service => service.GetDocumentByTitle(editDocumentDto.Title))
                .ReturnsAsync(existingDocument);

            // Act
            var result = await _controller.CreateDocument(editDocumentDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal($"There is already a document with the title {editDocumentDto.Title}.", ((ConflictObjectResult)result).Value);
        }

        [Fact]
        public async Task CreateDocument_ReturnsBadRequest_WhenCreationFails()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "New Document" };
            _documentServiceMock.Setup(service => service.CreateDocument(editDocumentDto))
                .ReturnsAsync((Document)null);

            // Act
            var result = await _controller.CreateDocument(editDocumentDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Something went wrong...", ((BadRequestObjectResult)result).Value);
        }

        // PUT /api/Document/{id}
        [Fact]
        public async Task UpdateDocument_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var invalidDto = new EditDocumentDto();
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            var result = await _controller.UpdateDocument(1, invalidDto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.IsType<SerializableError>(result.Value);
            var error = result.Value as SerializableError;
            Assert.Contains("Title", error.Keys);
        }

        [Fact]
        public async Task UpdateDocument_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "Updated Document", Content = "Updated Content", ContentTypeId = 1 };
            var existingDocumentDto = new DocumentDto
            {
                DocumentId = 1,
                Title = "Old Document",
                Content = "Old Content",
                ContentTypeId = 1,
                UserId = "test-user-id"
            };

            var updatedDocumentDto = new DocumentDto
            {
                DocumentId = 1,
                Title = "Updated Document",
                Content = "Updated Content",
                ContentTypeId = 1,
                UserId = "test-user-id"
            };

            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync(existingDocumentDto);
            _documentServiceMock.Setup(service => service.UpdateDocument(editDocumentDto, 1))
                .ReturnsAsync(new Document
                {
                    DocumentId = updatedDocumentDto.DocumentId,
                    Title = updatedDocumentDto.Title,
                    Content = updatedDocumentDto.Content,
                    ContentTypeId = updatedDocumentDto.ContentTypeId,
                    UserId = updatedDocumentDto.UserId
                });

            // Act
            var result = await _controller.UpdateDocument(1, editDocumentDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"Document with id {updatedDocumentDto.DocumentId} updated successfully", result.Value);
        }


        [Fact]
        public async Task UpdateDocument_ReturnsConflict_WhenDocumentWithSameTitleExists()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "Existing Document", Content = "Updated Content", ContentTypeId = 1 };
            var existingDocument = new Document
            {
                DocumentId = 2,
                Title = "Existing Document",
                Content = "Some Content",
                ContentTypeId = 1,
                UserId = "test-user-id"
            };

            _documentServiceMock.Setup(service => service.GetDocumentByTitle(editDocumentDto.Title))
                .ReturnsAsync(existingDocument);

            // Act
            var result = await _controller.UpdateDocument(1, editDocumentDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal($"There is already a document with the title {editDocumentDto.Title}.", ((ConflictObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateDocument_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "Non-existent Document" };
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync((DocumentDto)null);

            // Act
            var result = await _controller.UpdateDocument(1, editDocumentDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No document found for the document Id 1.", ((NotFoundObjectResult)result).Value);
        }
        
        [Fact]
        public async Task UpdateDocument_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var editDocumentDto = new EditDocumentDto { Title = "Updated Document", Content = "Content", ContentTypeId = 1 };
            var existingDocument = new DocumentDto { DocumentId = 1, Title = "Old Document", Content = "Old Content", ContentTypeId = 1 };
    
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync(existingDocument);
            _documentServiceMock.Setup(service => service.UpdateDocument(editDocumentDto, 1))
                .ReturnsAsync((Document?)null);

            // Act
            var result = await _controller.UpdateDocument(1, editDocumentDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Something went wrong...", result.Value);
        }

        // DELETE /api/Document/{id}
        [Fact]
        public async Task DeleteDocument_ReturnsNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            var existingDocument = new DocumentDto { DocumentId = 1, Title = "Test Document" };
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync(existingDocument);
            _documentServiceMock.Setup(service => service.DeleteDocument(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteDocument(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        
        [Fact]
        public async Task DeleteDocument_ReturnsBadRequest_WhenDeletionFails()
        {
            // Arrange
            var existingDocument = new DocumentDto { DocumentId = 1, Title = "Test Document", Content = "Content", ContentTypeId = 1 };
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync(existingDocument);
            _documentServiceMock.Setup(service => service.DeleteDocument(1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteDocument(1) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Either the file with id 1 does not exist or you do not have permission to delete the document.", result.Value);
        }

        [Fact]
        public async Task DeleteDocument_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            _documentServiceMock.Setup(service => service.GetDocumentById(1))
                .ReturnsAsync((DocumentDto)null);

            // Act
            var result = await _controller.DeleteDocument(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No document found for the document Id 1.", ((NotFoundObjectResult)result).Value);
        }
    }
}
