using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Serilog;
using System;
using System.Linq;

namespace Namingway;

internal class Commands : IDisposable
{
	private NamingwayPlugin Plugin { get; }

	internal Commands(NamingwayPlugin plugin)
	{
		this.Plugin = plugin;

		Service.CommandManager.AddHandler("/namingway", new CommandInfo(this.OnCommand)
		{
			HelpMessage = "Opens the Namingway interface",
		});
	}

	public void Dispose()
	{
		Service.CommandManager.RemoveHandler("/namingway");
	}

	private void OnCommand(string command, string arguments)
	{
		if (arguments.Length != 0)
			switch (arguments = arguments?.ToLowerInvariant() ?? string.Empty)
			{
				case null:
					this.Plugin.Ui.DrawSettings ^= true;
					break;

				case "help":
				case "disable":
				case "enable":
					Print("To enable or disable a pack, please provide its GUID. Example: /namingway <enable|disable> <GUID>.");
					break;

				case not null when arguments.Contains("toggle"):
					TogglePack(arguments.Split(" ")[1], true, true);
					break;

				case not null when arguments.Contains("enable"):
					TogglePack(arguments.Split(" ")[1], true);
					break;
				case not null when arguments.Contains("disable"):
					TogglePack(arguments.Split(" ")[1], false);
					break;
			}
		else
		{
			this.Plugin.Ui.DrawSettings ^= true;
		}

	}

	private void TogglePack(string packId, bool enable, bool toggle = false)
	{
		Pack pack;
		Guid packGuid;
		string chatPrint;
		try
		{
			packGuid = Guid.Parse(packId);
		}
		catch (Exception)
		{

			Print("GUID not found, or it's an incorrect one. Please provide a correct GUID.");
			return;
		}

		pack = Plugin.Config.FindEnabledPack(packGuid);

		var custom = DefaultPacks.All.All(p => p.Id != pack.Id);
		var enabled = this.Plugin.Config.EnabledPacks.Contains(pack.Id);

		if(toggle && enabled) enable = false;
		if(toggle && !enabled) enable = true;

		if (!enabled && enable)
		{
			this.Plugin.Config.EnabledPacks.Add(pack.Id);
			pack.Enable(this.Plugin.Renamer);

		}
		else if (enabled && !enable)
		{
			this.Plugin.Config.EnabledPacks.Remove(pack.Id);
			pack.Disable(this.Plugin.Renamer);
		}

		if (enable) Print($"Pack \"{pack.Name}\" is enabled.");
		else Print($"Pack \"{pack.Name}\" is disabled.");

		this.Plugin.Config.SaveConfig();
	}

	public static void Print(SeString message)
	{
		var entry = new XivChatEntry()
		{
			Message = "[Namingway] " + message,
			Name = SeString.Empty,
			Type =XivChatType.SystemMessage,
		};
		Service.Chat.Print(entry);
	}

}