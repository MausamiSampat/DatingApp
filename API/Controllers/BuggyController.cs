using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext context) : BaseApiController
{
    [HttpGet("auth")]
    public ActionResult<string> GetAuth()
    {
        return "Secret Text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var things = context.Users.Find(-1);
        if (things == null)
            return NotFound();
        return things;
    }

    [HttpGet("server-error")]
    public ActionResult<AppUser> GetServerError()
    {
        try
        {
            var things = context.Users.Find(-1) ?? throw new Exception("A bad thing had happened");

            return things;
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Computer says no");
        }

    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("Not a good request");
    }
}
