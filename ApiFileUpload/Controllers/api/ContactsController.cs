using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ApiFileUpload.Models;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;

namespace ApiFileUpload.Controllers.api
{
    [RoutePrefix("api/Contacts")]
    public class ContactsController : ApiController
    {
        private PhoneBookEntities db = new PhoneBookEntities();

        // GET: api/Contacts
        public IQueryable<UserContact> GetUserContacts()
        {
            return db.UserContacts;
        }

        // GET: api/Contacts/5
        [ResponseType(typeof(UserContact))]
        public async Task<IHttpActionResult> GetUserContact(int id)
        {
            UserContact userContact = await db.UserContacts.FindAsync(id);
            if (userContact == null)
            {
                return NotFound();
            }

            return Ok(userContact);
        }

        // PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUserContact(int id, UserContact userContact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userContact.UserID)
            {
                return BadRequest();
            }

            db.Entry(userContact).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Contacts
        [ResponseType(typeof(UserContact))]
        public async Task<IHttpActionResult> PostUserContact(UserContact userContact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UserContacts.Add(userContact);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserContactExists(userContact.UserID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = userContact.UserID }, userContact);
        }

        // POST: api/Contacts
        [HttpPost, Route("SaveUserContact")]
        public async Task<IHttpActionResult> SaveUserContact()
        {
            var userContact = new UserContact();
            var httpRequest = HttpContext.Current.Request;
            var requestedFiles = httpRequest.Files;
            NameValueCollection nvc = httpRequest.Form;

            try
            {
                //http://stackoverflow.com/questions/30490313/post-json-with-data-and-file-to-web-api-jquery-mvc
                foreach (string kvp in nvc.AllKeys)
                {
                    PropertyInfo pi = userContact.GetType().GetProperty(kvp, BindingFlags.Public | BindingFlags.Instance);
                    if (pi != null)
                    {
                        pi.SetValue(userContact, nvc[kvp], null);
                    }
                }

                foreach (string file in requestedFiles)
                {
                    var postedFile = httpRequest.Files[file];
                    var filePath = HttpContext.Current.Server.MapPath("~/Content/Uploaded/" + postedFile.FileName);
                    postedFile.SaveAs(filePath);
                    switch (file)
                    {
                        case "Avatar":
                            userContact.Avatar = postedFile.FileName;
                            break;
                        case "Signature":
                            userContact.Signature = postedFile.FileName;
                            break;
                        default:
                            break;
                    }
                }

                //Save Data to Database
                db.UserContacts.Add(userContact);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserContactExists(userContact.UserID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            return Json("Registered!! Boom!!!");
        }

        // DELETE: api/Contacts/5
        [ResponseType(typeof(UserContact))]
        public async Task<IHttpActionResult> DeleteUserContact(int id)
        {
            UserContact userContact = await db.UserContacts.FindAsync(id);
            if (userContact == null)
            {
                return NotFound();
            }

            db.UserContacts.Remove(userContact);
            await db.SaveChangesAsync();

            return Ok(userContact);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserContactExists(int id)
        {
            return db.UserContacts.Count(e => e.UserID == id) > 0;
        }
    }
}