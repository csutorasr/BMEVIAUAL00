﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Repository.Writers;
using WebInterface.Model;
using WebInterface.Repository.Writings;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebInterface.API
{
    [Route("api/[controller]")]
    public class Writers : Controller
    {
        private IWriterRepository writerRepository;
        private IWritingRepository writingRepository;

        public Writers(IWriterRepository writerRepository, IWritingRepository writingRepository)
        {
            this.writerRepository = writerRepository;
            this.writingRepository = writingRepository;
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return writerRepository.GetList();
        }
        [HttpGet("{id}")]
        public Writer Get(string id)
        {
            return writerRepository.Get(id);
        }
        [HttpGet("{id}/{writingId}")]
        public Writing Get(string id, string writingId)
        {
            return writingRepository.Get(id, writingId);
        }
        [HttpPut("{id}/{writingId}")]
        public IActionResult Put(string id, string writingId, [FromBody] ManualHandednessData data)
        {
            if (data.manualHandedness == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No type is specified");
            }
            return Ok(writingRepository.Set(id, writingId, data.manualHandedness));
        }

        public class ManualHandednessData
        {
            public string manualHandedness { get; set; }
        }

        [HttpPut("{id}/{writingId}/lines/{index}")]
        public IActionResult PutLine(string id, string writingId, int index, [FromBody] TypeRequestData data)
        {
            if (data.type == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No type is specified");
            }
            return Ok(writingRepository.SetLine(id, writingId, index, data.type));
        }

        public class TypeRequestData
        {
            public string type { get; set; }
        }
        [HttpDelete("{id}/{writingId}/lines/{index}")]
        public IActionResult DeleteLine(string id, string writingId, int index)
        {
            return Ok(writingRepository.RemoveLine(id, writingId, index));
        }
    }
}
