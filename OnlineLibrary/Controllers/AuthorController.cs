using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineLibrary.Data;
using OnlineLibrary.Services.Core.Exceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Controllers
{
    public class AuthorController : BaseController
    {
        private readonly OnlineLibraryDbContext dbContext;
        private readonly IAuthorService authorService;
        private readonly ILogger<AuthorController> logger;
        public AuthorController(OnlineLibraryDbContext dbContext, IAuthorService authorService, ILogger<AuthorController> logger)
        {
            this.dbContext = dbContext;
            this.authorService = authorService;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            var model = await authorService.GetAllAuthorsAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var authorModel = await authorService.GetAuthorByIdAsync(id);

            if (authorModel == null)
            {
                return NotFound();
            }
            return View(authorModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var model = authorService.GetEmtyAuthorFormModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AuthorAddViewModel inputModel)
        {
            var model = authorService.GetEmtyAuthorFormModelAsync();
            model = inputModel;

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                await authorService.AddAuthorAsync(inputModel);
                return RedirectToAction("All", "Author");
            }
            catch (AuthorAlreadyExistsException ex)
            {
                logger.LogWarning(ex, "Attempt to add existing author {FullName}", inputModel.FullName);
                // bind error to the FullName field so user sees the specific issue
                ModelState.AddModelError(nameof(AuthorAddViewModel.FullName), ex.Message);
                return View(inputModel);
            }
            catch (AuthorCreateException ex)
            {
                logger.LogError(ex, "An error occurred while adding a new author.");
                ModelState.AddModelError(string.Empty, "An error occurred while adding the author. Please try again.");
                return View(inputModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while adding author.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");
                return View(inputModel);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = new AuthorEditViewModel();
            try
            {
                model = await authorService.GetAuthorForEditByIdAsync(id);
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "Attempt to edit non-existing author with id {AuthorId}", id);
                ModelState.AddModelError(string.Empty, "The author you are trying to edit does not exist.");
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while loading edit form for author with id {AuthorId}.", id);
                return StatusCode(500, "An unexpected error occurred. Please contact support.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, AuthorEditViewModel inputModel)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }


            try
            {
                await authorService.UpdateAuthorAsync(id, inputModel);
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "Attempt to update non-existing author with id {AuthorId}", id);
                ModelState.AddModelError(string.Empty, "The author you are trying to update does not exist.");
                return NotFound();
            }
            catch (AuthorUpdateExeption ex)
            {
                logger.LogError(ex, "An error occurred while updating author with id {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the author. Please try again.");
                return View(inputModel);
            }
            catch
            {
                logger.LogError("Unexpected error while updating author with id {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the author. Please contact support.");
                return View(inputModel);
            }
            return RedirectToAction("All", "Author");
        }


        //try
        //{
        //    await authorService.UpdateAuthorAsync(inputModel);
        //    return RedirectToAction("All", "Author");
        //}
        //catch (AuthorAlreadyExistsException ex)
        //{
        //    logger.LogWarning(ex, "Attempt to add existing author {FullName}", inputModel.FullName);
        //    // bind error to the FullName field so user sees the specific issue
        //    ModelState.AddModelError(nameof(AuthorAddViewModel.FullName), ex.Message);
        //    return View(inputModel);
        //}
        //catch (AuthorCreateException ex)
        //{
        //    logger.LogError(ex, "An error occurred while adding a new author.");
        //    ModelState.AddModelError(string.Empty, "An error occurred while adding the author. Please try again.");
        //    return View(inputModel);
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex, "Unexpected error while adding author.");
        //    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");
        //    return View(inputModel);
        //}

        //return View(inputModel);




    }
}

