﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDiscordSoundboard.Models;
using TheDiscordSoundboard.Models.Bot;
using TheDiscordSoundboard.Service;

namespace TheDiscordSoundboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController: ControllerBase
    {
        // bot has full access to database
        // however only queue and credentials are used
        private readonly IBotService _service;

        public BotController(IBotService service)
        {
            _service = service;
        }


        [HttpGet]
        public ActionResult<BotState> GetBot()
        {
            return _service.GetBot();
        }


        [Route("connect")]
        [HttpGet]
        public async Task ConnectBot()
        {
            await _service.ConnectBot(); 
            
        }


        [Route("queue")]
        [HttpGet]
        public ActionResult<List<BotTrackData>> GetBotQueue()
        {
            return _service.GetQueue();
        }


        [Route("queue")]
        [HttpPut]
        public async Task<IActionResult> PutBotQueue(List<BotTrackData> queue)
        {
            await _service.UpdateQueue(queue);
            return NoContent();
        }


        [Route("queue/item")]
        [HttpPost]
        public async Task<BotTrackData> AppendBotQueue(BotTrackData append)
        {
            await _service.AppendToQueue(append);
            return append;
        }

    }
}
