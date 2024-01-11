using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

// make data validation easier, check required properties and return a bad request if fails validation
[ApiController]
[Route("api/auctions")]
public class AuctionsControllers : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly AuctionDbContext _context;

    // context is used to query the database
    // mapper is used to map between DTOs and Entities
    public AuctionsControllers(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ActionResult sends back Http responses such as 202, 404, 500, etc
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction is null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // add current user as seller;
        auction.Seller = "test";

        // add item to memory
        _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Could not save changes to the DB");
        }

        // this is the location you can get your resource (GetAuctionById) -> return the location which you can get the resource
        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    // don't need to return anything, just update the auction
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions // query the database
            .Include(x => x.Item) // this will load the related properties (Item) without having to make another query
            .FirstOrDefaultAsync(x => x.Id == id); // find the auction with the id

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

        var result = await _context.SaveChangesAsync() > 0; // save changes to the database, why greater than 0? if it's 0, then nothing was changed
        if (result)
        {
            return Ok();
        }
        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id); // find the auction with the id, this does not automatically load Item property

        if (auction is null)
        {
            return NotFound();
        }
        // TODO: check seller == username

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0; // save changes to the database, why greater than 0? if it's 0, then nothing was changed
        if (result)
        {
            return Ok();
        }
        return BadRequest("Problem deleting auction");
    }
}
