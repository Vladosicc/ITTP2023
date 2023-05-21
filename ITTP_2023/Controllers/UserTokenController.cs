using ITTP_2023.Models.Errors;
using ITTP_2023.Models.FromBody;
using ITTP_2023.Models;
using ITTP_2023.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ITTP_2023.Models.ForResponse;

namespace ITTP_2023.Controllers
{
    /// <summary>
    /// API с использованием токена авторизации. Для авторизации использовать стандартную форму из swagger.
    /// </summary>
    [Route("api/[controller]")]
    public class UserTokenController : Controller
    {
        /// <summary>
        /// Имя токена в Headers
        /// </summary>
        const string _tokenName = "Token";

        readonly IUserService _service;

        public UserTokenController(IUserService service)
        {
            _service = service;
        }

        #region GET

        /// <summary>
        ///  Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn(Доступно Админам)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     GET /ReadActiveUsers?login=Admin&#38;password=Admin
        ///
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpGet("ReadActiveUsers")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ReadActiveUsers()
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var user = await _service.LogInByTokenAsync(value);
                return Ok(await _service.GetActiveUsersAsync(user));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный или нет(Доступно Админам)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     GET /ReadUserByLogin?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин для поиска</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpGet("ReadUserByLogin")]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ReadUserByLogin(string loginRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);
                return Ok(new UserResponse(await _service.GetUserByLoginAsync(editor, loginRequest)));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     GET /Login?login=Admin&#38;password=Admin
        ///
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpGet("Login")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Login()
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var user = await _service.LogInByTokenAsync(value);

                if (user == null)
                    throw new Exception("Неверный логин и/или пароль");
                if (user.RevokedOn.HasValue)
                    throw new Exception("Аккаунт заблокирован");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Запрос всех пользователей старше определённого возраста (Доступно Админам)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     GET /ReadOlderThan?login=Admin&#38;password=Admin&#38;age=18
        ///
        /// </remarks>
        /// <param name="age">Возраст</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpGet("ReadOlderThan")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ReadOlderThan(int age)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.GetUsersOlderAsync(editor, age));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        #endregion

        #region POSTCreate

        /// <summary>
        /// Создание пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     POST /Create
        ///     {
        ///        "login" : "Andrew123",
        ///        "password" : "qwerty",
        ///        "name": "Andrew",
        ///        "gender": 1,
        ///        "birthday": "5/1/2008 8:30:52 AM",
        ///        "isAdmin": "true"
        ///     }
        ///
        /// </remarks>
        /// <param name="user">Информация о новом пользователе</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Create([FromBody] UserRequest user)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.CreateAsync(editor, user.Login, user.Password, user.Name, user.Gender, user.Birthday, user.Admin));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        #endregion POSTCreate

        #region PUT

        /// <summary>
        /// Удаление пользователя по логину мягкое происходить простановка RevokedOn и RevokedBy (Доступно Админам) 
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     PUT /Block?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин для блокировки</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPut("Block")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Block(string loginRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.DeleteSoftAsync(editor, loginRequest));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Восстановление пользователя - Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     PUT /Unblock?login=Admin&#38;#38;password=Admin&#38;#38;loginRequest=Vasya123
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин разблокировки</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPut("Unblock")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Unblock(string loginRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.UnBlockAsync(editor, loginRequest));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     PUT /Update?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123&#38;name=Vasyliy&#38;gender=1
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин изменяемого пользователя (заполняется, если администратор изменяет другого пользователя, иначе оставляется пустым)</param>
        /// <param name="name">Новое имя пользователя (не заполнять, если не нужно изменять)</param>
        /// <param name="gender">Пол (не заполнять, если не нужно изменять)</param>
        /// <param name="birthday">Дата рождения (не заполнять, если не нужно изменять)</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPut("Update")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Update(string loginRequest, string name, int? gender, DateTime? birthday)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.UpdateAsync(editor, loginRequest ?? editor.Login, name, gender, birthday));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn), логин должен оставаться уникальным)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     PUT /ChangeLogin?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123&#38;newLogin=Vasya321
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин изменяемого пользователя (заполняется, если администратор изменяет другого пользователя, иначе оставляется пустым)</param>
        /// <param name="newLogin">Новый логин</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPut("ChangeLogin")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ChangeLogin(string loginRequest, string newLogin)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.UpdateLoginAsync(editor, loginRequest ?? (editor != null ? editor.Login : string.Empty), newLogin));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     PUT /ChangePassword?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123&#38;newPassword=123456
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин изменяемого пользователя (заполняется, если администратор изменяет другого пользователя, иначе оставляется пустым)</param>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpPut("ChangePassword")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ChangePassword(string loginRequest, string newPassword)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.UpdatePasswordAsync(editor, loginRequest ?? editor.Login, newPassword));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        #endregion PUT

        #region DELETE

        /// <summary>
        /// Удаление пользователя по логину полное (Доступно Админам)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     DELETE /Delete?login=Admin&#38;password=Admin&#38;loginRequest=Vasya123
        ///
        /// </remarks>
        /// <param name="loginRequest">Логин для полного удаления</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API</response>
        [HttpDelete("Delete")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Delete(string loginRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_tokenName, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LogInByTokenAsync(value);

                return Ok(await _service.DeleteHardAsync(editor, loginRequest));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorMessage() { Message = ex.Message, ExceptionType = ex.GetType().Name });
            }
        }

        #endregion DELETE
    }
}
