using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Fortnite_API;
using Fortnite_API.Objects;
using Fortnite_API.Objects.V2;

namespace CosmeticBot.Modules
{
	public class Commands : ModuleBase<SocketCommandContext>
	{
		private readonly FortniteApi fortniteApi;
		private readonly CommandService commandService;

		public Commands(FortniteApi fortniteApi, CommandService commandService)
		{
			this.fortniteApi = fortniteApi;
			this.commandService = commandService;
		}

		[Command("aes")]
		public async Task aes()
		{
			var aes = await fortniteApi.V2.Aes.GetAsync();

			if (aes.HasError)
			{
				EmbedBuilder builder = new EmbedBuilder()
					.WithTitle("Unknown error occured.")
					.WithColor(new Color(0xFF0000))
					.AddField("Error", char.ToUpper(aes.Error[0]) + aes.Error.Substring(1) + '.');
				await ReplyAsync(embed: builder.Build());
			}
			else
			{
				EmbedBuilder builder = new EmbedBuilder()
					.WithTitle("AES")
					.WithColor(new Color(0xE83E8C))
					.AddField("Key", aes.Data.MainKey);

				await ReplyAsync(embed: builder.Build());
			}
		}

		/*[Command("shop")]
		public async Task shop()
		{
			var shop = await fortniteApi.V2.Shop.GetBrAsync(); ;

			if (shop.HasError)
			{
				EmbedBuilder builder = new EmbedBuilder()
					.WithTitle("Unknown error occured.")
					.WithColor(new Color(0xFF0000))
					.AddField("Error", char.ToUpper(shop.Error[0]) + shop.Error.Substring(1) + '.');
				await ReplyAsync(embed: builder.Build());
			}
			else
			{
				EmbedBuilder featured = new EmbedBuilder()
					.WithTitle("Featured Items")
					.WithColor(new Color(0xE83E8C));


				foreach (string item in shop.Data.Featured.Entries.);

				await ReplyAsync(embed: builder.Build());
			}

		}
		*/
	}
}
