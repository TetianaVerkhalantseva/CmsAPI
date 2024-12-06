using CmsAPI.Controllers;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.FolderServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CmsAPITests
{
    public class FolderControllerTests
    {
        private readonly Mock<IFolderService> _folderServiceMock;
        private readonly FolderController _controller;

        public FolderControllerTests()
        {
            _folderServiceMock = new Mock<IFolderService>();
            _controller = new FolderController(_folderServiceMock.Object);
        }
        
        // GET /api/Folder/user-folders
        [Fact]
        public async Task GetUserFolders_ReturnsOk_WhenFoldersExist()
        {   
            // Arrange
            var folders = new List<FolderDto> { new FolderDto { FolderId = 1, FolderName = "Test Folder" } };
            _folderServiceMock.Setup(service => service.GetFoldersByUserId())
                .ReturnsAsync(folders);

            // Act
            var result = await _controller.GetUserFolders() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(folders, result.Value);
        }

        [Fact]
        public async Task GetUserFolders_ReturnsNotFound_WhenNoFoldersExist()
        {
            // Arrange
            _folderServiceMock.Setup(service => service.GetFoldersByUserId())
                .ReturnsAsync(new List<FolderDto>());
            
            // Act
            var result = await _controller.GetUserFolders();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No folders found for the user.", ((NotFoundObjectResult)result).Value);
        }
        
        // GET /api/Folder/{id}
        [Fact]
        public async Task GetFolderById_ReturnsOk_WhenFolderExists()
        {
            // Arrange
            var folder = new FolderDto { FolderId = 1, FolderName = "Test Folder" };
            _folderServiceMock.Setup(service => service.GetFolderById(1))
                .ReturnsAsync(folder);
            
            // Act
            var result = await _controller.GetFolderById(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetFolderById_ReturnsNotFound_WhenFolderDoesNotExist()
        {
            // Arrange
            _folderServiceMock.Setup(service => service.GetFolderById(1))
                .ReturnsAsync((FolderDto?)null);

            // Act
            var result = await _controller.GetFolderById(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Folder with Id 1 not found.", ((NotFoundObjectResult)result).Value);
        }
        
        // POST /api/Folder
        [Fact]
        public async Task CreateFolder_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidDto = new FolderInputDto();
            _controller.ModelState.AddModelError("FolderName", "The FolderName field is required.");

            // Act
            var result = await _controller.CreateFolder(invalidDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SerializableError>(result.Value);
            var error = result.Value as SerializableError;
            Assert.Contains("FolderName", error.Keys);
        }

        [Fact]
        public async Task CreateFolder_ReturnsCreatedAtAction_WhenFolderIsValid()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "New Folder" };
            var createdFolder = new Folder { FolderId = 1, FolderName = "New Folder" };

            _folderServiceMock.Setup(service => service.GetFolderByFolderName(inputDto.FolderName))
                .ReturnsAsync((Folder?)null);
            _folderServiceMock.Setup(service => service.CreateFolder(inputDto))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = true, UpdatedFolder = createdFolder });

            // Act
            var result = await _controller.CreateFolder(inputDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GetFolderById", result.ActionName);
        }

        [Fact]
        public async Task CreateFolder_ReturnsConflict_WhenFolderWithSameNameExists()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "Existing Folder" };
            var existingFolder = new Folder { FolderId = 1, FolderName = "Existing Folder" };

            _folderServiceMock.Setup(service => service.GetFolderByFolderName(inputDto.FolderName))
                .ReturnsAsync(existingFolder);

            // Act
            var result = await _controller.CreateFolder(inputDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal($"There is already a folder with the name {inputDto.FolderName}.", ((ConflictObjectResult)result).Value);
        }
        
        [Fact]
        public async Task CreateFolder_ReturnsNotFound_WhenParentFolderIsMissing()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "New Folder", ParentFolderId = 999 };
            _folderServiceMock.Setup(service => service.CreateFolder(inputDto))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = false, ErrorMessage = "Parent folder not found." });

            // Act
            var result = await _controller.CreateFolder(inputDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Parent folder not found.", ((NotFoundObjectResult)result).Value);
        }

        [Fact]
        public async Task CreateFolder_ReturnsBadRequest_WhenCreationFails()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "New Folder" };
            _folderServiceMock.Setup(service => service.CreateFolder(inputDto))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = false, ErrorMessage = "Error occurred." });

            // Act
            var result = await _controller.CreateFolder(inputDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error occurred.", ((BadRequestObjectResult)result).Value);
        }
        
        // PUT /api/Folder/{id}
        [Fact]
        public async Task UpdateFolder_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidDto = new FolderInputDto();
            _controller.ModelState.AddModelError("FolderName", "The FolderName field is required.");

            // Act
            var result = await _controller.UpdateFolder(1, invalidDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SerializableError>(result.Value);
            var error = result.Value as SerializableError;
            Assert.Contains("FolderName", error.Keys);
        }

        [Fact]
        public async Task UpdateFolder_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "Updated Folder" };
            _folderServiceMock.Setup(service => service.UpdateFolder(inputDto, 1))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = true });

            // Act
            var result = await _controller.UpdateFolder(1, inputDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Folder updated successfully.", result.Value);
        }

        [Fact]
        public async Task UpdateFolder_ReturnsNotFound_WhenFolderDoesNotExist()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "Non-existent Folder" };
            _folderServiceMock.Setup(service => service.UpdateFolder(inputDto, 1))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = false, ErrorMessage = "Folder with Id 1 not found." });

            // Act
            var result = await _controller.UpdateFolder(1, inputDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Folder with Id 1 not found.", ((NotFoundObjectResult)result).Value);
        }
        
        [Fact]
        public async Task UpdateFolder_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var inputDto = new FolderInputDto { FolderName = "Updated Folder" };
            _folderServiceMock.Setup(service => service.UpdateFolder(inputDto, 1))
                .ReturnsAsync(new FolderUpdateResult { IsSuccess = false, ErrorMessage = "An unexpected error occurred." });

            // Act
            var result = await _controller.UpdateFolder(1, inputDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An unexpected error occurred.", result.Value);
        }
        
        // DELETE /api/Folder/{id}
        [Fact]
        public async Task DeleteFolder_ReturnsOk_WhenDeletionIsSuccessful()
        {
            // Arrange
            _folderServiceMock.Setup(service => service.GetFolderById(1))
                .ReturnsAsync(new FolderDto { FolderId = 1, FolderName = "Test Folder" });
            _folderServiceMock.Setup(service => service.DeleteFolder(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteFolder(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Folder successfully deleted.", ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task DeleteFolder_ReturnsNotFound_WhenFolderDoesNotExist()
        {
            // Arrange
            _folderServiceMock.Setup(service => service.GetFolderById(1))
                .ReturnsAsync((FolderDto?)null);

            // Act
            var result = await _controller.DeleteFolder(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Folder with Id 1 not found.", ((NotFoundObjectResult)result).Value);
        }

        [Fact]
        public async Task DeleteFolder_ReturnsBadRequest_WhenDeletionFails()
        {
            // Arrange
            _folderServiceMock.Setup(service => service.GetFolderById(1))
                .ReturnsAsync(new FolderDto { FolderId = 1, FolderName = "Test Folder" });
            _folderServiceMock.Setup(service => service.DeleteFolder(1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteFolder(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("An error occurred while deleting the folder.", ((BadRequestObjectResult)result).Value);
        }
        
        // GET /api/Folder/root
        [Fact]
        public async Task GetRootFolder_ReturnsOk_WithRootFolderData()
        {
            // Arrange
            var rootFolder = new RootFolderModel();
            _folderServiceMock.Setup(service => service.GetRootFolder())
                .ReturnsAsync(rootFolder);

            // Act
            var result = await _controller.GetRootFolder() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rootFolder, result.Value);
        }
    }
}
