from discord.ext import commands
import discord

import aiohttp
import json
import datetime

FORTNITE_API_BASE = 'https://fortnite-api.com/'

with open('tokens.json') as f:
    data = json.load(f)


# Credit to Terbau for this function.
def from_iso(iso: str) -> datetime.datetime:
    """:class:`str`: Converts an iso formatted string to a
    :class:`datetime.datetime` object
    Returns
    -------
    :class:`datetime.datetime`
    """
    if isinstance(iso, datetime.datetime):
        return iso

    try:
        return datetime.datetime.strptime(iso, '%Y-%m-%dT%H:%M:%S.%fZ')
    except ValueError:
        return datetime.datetime.strptime(iso, '%Y-%m-%dT%H:%M:%SZ')


async def fortnite_api_request(url: str, params: dict = {}) -> dict:
    async with aiohttp.ClientSession() as session:
        async with session.request(
            method='GET',
            url=f'{FORTNITE_API_BASE}{url}',
            params=params,
            headers={"x-api-key": data['fnapi-api-key']},
        ) as r:
            return await r.json()

bot = commands.Bot(
    command_prefix='.',
    description='Discord bot powered by Fortnite-API.com',
    case_insensitive=True
)


@bot.event
async def on_ready() -> None:
    print(f'FortniteAPI-Bot ready as {bot.user.name}.')


@bot.event
async def on_message(message) -> None:
    await bot.process_commands(message)


@bot.command()
async def aes(ctx) -> None:
    """Returns the current aes key."""
    response = await fortnite_api_request('aes')

    status = response.get('status', 400)

    if status == 200:
        # last_update = from_iso(response['data']['lastUpdate']).strftime("%b %d %Y %H:%M:%S")

        embed = discord.Embed(
            title=f"AES for {response['data']['build'].split('Release-')[1].split('-CL-')[0]}.",
            colour=0xE83E8C
        )

        embed.add_field(name="AES", value=response['data']['aes'])
        embed.add_field(name="Build", value=response['data']['build'], inline=False)

        embed.set_footer(
            text=f"Last Update: {response['data']['lastUpdate']}",
            icon_url="https://fortnite-api.com/logo.png"
        )

        await ctx.send(embed=embed)

    elif status == 401:
        embed = discord.Embed(
            title=f"Invalid/Missing Fortnite-API API key.",
            colour=0xFF0000
        )

        embed.add_field(
            name="Error",
            value=response['error']
        )

        embed.add_field(
            name="Get API key",
            value='You can get a new api key from the [Fortnite-API website](https://fortnite-api.com/).',
            inline=False
        )

        await ctx.send(embed=embed)

    else:
        embed = discord.Embed(
            title=f"Unknown error occurred.",
            colour=0xFF0000
        )

        embed.add_field(
            name="Error",
            value=response.get('error', 'No error provided.')
        )

        await ctx.send(embed=embed)


@bot.command()
async def shop(ctx, lang: str = 'en') -> None:
    """Returns data of the current battle royale item shop"""
    response = await fortnite_api_request('shop/br', params={'language': lang})

    status = response.get('status', 400)

    if status == 200:
        featured = discord.Embed(title=f"Featured Items", colour=0xE83E8C)

        for featured_item in response['data']['featured']:
            featured.add_field(
                name=featured_item['items'][0]['name'],
                value=f"Price: {featured_item['finalPrice']} V-Bucks"
            )

        daily = discord.Embed(title=f"Daily Items", colour=0xE83E8C)

        for daily_item in response['data']['daily']:
            daily.add_field(
                name=daily_item['items'][0]['name'],
                value=f"Price: {daily_item['finalPrice']} V-Bucks"
            )

        await ctx.send(embed=featured)
        await ctx.send(embed=daily)

    elif status == 401:
        embed = discord.Embed(
            title=f"Invalid/Missing Fortnite-API API key.",
            colour=0xFF0000
        )

        embed.add_field(
            name="Error",
            value=response['error']
        )

        embed.add_field(
            name="Get API key",
            value='You can get a new api key from the [Fortnite-API website](https://fortnite-api.com/).',
            inline=False
        )

        await ctx.send(embed=embed)

    else:
        embed = discord.Embed(
            title=f"Unknown error occurred.",
            colour=0xFF0000
        )

        embed.add_field(
            name="Error",
            value=response.get('error', 'No error provided.')
        )

        await ctx.send(embed=embed)




bot.run(data['discord-token'])
