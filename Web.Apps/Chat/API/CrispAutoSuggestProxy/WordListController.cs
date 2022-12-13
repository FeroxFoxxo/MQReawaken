using Microsoft.AspNetCore.Mvc;
using Web.Apps.Chat.Services;

namespace Web.Apps.Chat.API.CrispAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/WordList")]
public class WordListController : Controller
{
    private readonly ChatHandler _chatHandler;

    public WordListController(ChatHandler chatHandler) => _chatHandler = chatHandler;

    [HttpGet]
    public IActionResult GetWordsList() => File(_chatHandler.EncryptedWordList, "application/octet-stream");
}
