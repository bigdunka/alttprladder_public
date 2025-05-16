const { Client, GatewayIntentBits, Collection, Events, ModalBuilder, EmbedBuilder, ButtonBuilder, ButtonStyle, SlashCommandBuilder, ActionRowBuilder, StringSelectMenuBuilder, SelectMenuBuilder, TextInputBuilder, TextInputStyle, GuildScheduledEventManager, ChannelType, PermissionFlagsBits } = require('discord.js');
const client = new Client({ intents: [ GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.GuildEmojisAndStickers, GatewayIntentBits.GuildMessageReactions, GatewayIntentBits.GuildPresences, GatewayIntentBits.GuildMembers, GatewayIntentBits.MessageContent, GatewayIntentBits.GuildScheduledEvents ] });
const invites = new Collection();

var fs = require('fs');
var request = require('request');
var latinize = require('latinize');

//Globals
var hash = 'REDACTED';
var apilocation = 'REDACTED';
var guild;
var activeappeal = false;
var hasbeeninitialized = false;
var timerneedsreset = false;

//Log Files
var basedir = "c:\\bots\\lazykid\\logs\\"

//User IDs
var lazykidid = '680473858033188877';
var dunkaid = '219930246097534976';

//Server IDs
var guildid = '680412815697117215';

//Channel IDs
var serverannouncementid = '680412815697117225'; //#announcements
var ladderregistrationid = '1125828160429228092'; //#ladder-registration
var racingannouncementid = '680484352580255871'; //#race-announcements
var racesignupid = '680472639936266279'; //#race-signup
var faqid = '688946783161942061'; //#faq
var appealsid = '706311049829416990'; //#appeals
var appealrequestsid = '706311088668540989'; //#appeal-requests
var streamsid = '1171837927211405404'; //#streams
var welcomemessagesid = '839317581802242058'; //#welcome-messages
var restrictedaccessid = '965991435969101865'; //#restricted-access
var createaseed = '1123770249087549513'; //#create-a-seed
var ladderpollingid = '1244672955347370138'; //#ladder-polling-station
var botcommandsid = '1271596343059677194'; //#bot-commands

//Category IDs
var liveracescatid = '1267131201378979954'; //LIVE RACES

//Roles IDs
var racerrole;
var adminrole;
var racingannouncementsid = '680510682076413997';
var openannouncementid = '695759667057524767';
var ambrosiaannouncementid = '695759875497787453';
var adkeysanityannouncementid = '695759951188066405';
var mysteryannouncementid = '695760029839523890';
var crosskeyannouncementid = '695760069450530888';
var invertedkeysanityannouncementid = '696027286474063942';
var casualbootsannouncementid = '716288690342592635';
var enemizerannouncementid = '716288870521634837';
var openkeysanityannouncementid = '737836911237922827';
var reducedcrystalsannouncementid = '763556901567856662';
var invrosiaannouncementid = '763557052525707274';
var standardannouncementid = '789324118323232779';
var spoileropenannouncementid = '803283353885343795';
var mcshuffleannouncementid = '809849463681187891';
var s7potpourriannouncementid = '833464729091309568';
var retranceannouncementid = '833464889360121906';
var swordlessannouncementid = '850516152144887849';
var triforcehuntannouncementid = '895303001428881488';
var grabbagannouncementid = '895303111197986897';
var ludicrousspeedannouncementid = '923589479862763533';
var ambroz1aannouncementid = '969619622955462708';
var championshuntannouncementid = '1023230219663970414';
var insanityentranceannouncementid = '1049121766657769572';
var patronpartyannouncementid = '1113654256352899092';
var hundokeysanityannouncementid = '1113654111850741841';
var doubledownannouncementid = '1235713362982342677';
var championsmysteryannouncementid = '1235713279058514041';
var doorsannouncementid = '1295894882610843670';
var potteryannouncementid = '1295895414444265483';
var keydropannouncementid = '1296148459186819132';
var adminid = '680413813493071882';
var restrictedid = '965991548615557130';

//Race Arrays
var signupschedule;
var leavecutoff = false;
var racename = ['', ''];
var raceroom = [null, null];
var raceprerace = [null, null];
var racelounge = [null, null];
var currentlyracing = [0, 0];
var signedup = [0, 0];
var racestandings = [null, null, null, null];
var racerole = [null, null];
var loungerole = [null, null];
var spoilerhash = ['', ''];
var racehours = [0, 0];
var raceinprogress = [null, null];

//Message IDs
var restrictedmessageid = '1110546072956313600'

client.login('REDACTED');

client.on('ready', async () => {
	console.log(`Logged in as ${client.user.tag} - ` + new Date());
	
	if (hasbeeninitialized === false) {
		hasbeeninitialized = true;

		//Declare global objects
		guild = client.guilds.cache.get(guildid);
		racesignup = guild.channels.cache.get(racesignupid);
		adminrole = guild.roles.cache.find(role => role.name == "Admins");
		client.channels.cache.get(restrictedaccessid).messages.fetch(restrictedmessageid);
		racerrole = guild.roles.cache.find(role => role.name === "Racer");

		const firstInvites = await guild.invites.fetch();
		invites.set(guild.id, new Collection(firstInvites.map((invite) => [invite.code, invite.uses])));
		
		//GetNextRaces(true, true);
		GetNextRaces(false, true);
	}
});

client.on('error', async (error) => {
  if (error.code === 503) { // Service Unavailable
    console.error('Service Unavailable error encountered. Retrying...');

    let retryCount = 0;
    const maxRetries = 3; // Adjust as needed
    const retryInterval = 1000; // Initial retry interval in milliseconds

    while (retryCount < maxRetries) {
      try {
        await new Promise(resolve => setTimeout(resolve, retryInterval * Math.pow(2, retryCount))); // Exponential backoff
        // Perform the action that failed here (e.g., sending a message)
        console.log('Retry successful');
        break;
      } catch (retryError) {
        console.error(`Retry ${retryCount + 1} failed:`, retryError);
        retryCount++;
      }
    }

    if (retryCount === maxRetries) {
      console.error('Max retries reached. Giving up.');
      // Optionally handle the error further, e.g., logging to a database
    }
  }
});

//*****TRIGGERS BEGIN

client.on("messageCreate", (message) => {
	try
	{
		if (message.channel.type === 0 || message.channel.type === 11) {
			// ### GLOBAL LOGGING START ###
			var name = GetDisplayNickname(message.member);
			var channel;
			
			//Channel
			if (message.channel.type === 0) {
				channel = message.guild.channels.cache.find(channel => channel.id === message.channelId);
			//Thread
			} else if (message.channel.type === 11) {
				channel = message.guild.channels.cache.find(channel => channel.id === message.channel.parentId);
			}
			
			if (channel != null) {
				logMessage(channel.name, name, message.content);
			}
			// ### GLOBAL LOGGING END ###			
			
			var n = GetDisplayNickname(message.member);
			
			if (message.content.toLowerCase().indexOf('sahabot') > -1 && message.member.user.id === '219930246097534976') {
				message.channel.send("https://tenor.com/view/who-gif-4519872").catch(err => err);
			}
			
			//If Bot Overrides
			else if (message.channel.id === botcommandsid) {
				if (message.content.substring(0, 1) == '!') {
					var args = message.content.substring(1).split(' ');
					var cmd = args[0].toLowerCase();
					
					args = args.splice(1);	
					
					switch(cmd) {
						case 'regenerateseeds':
							RegenerateSeeds();
							break;
						case 'calculaterankings':
							CalculateRankings();
							break;
						case 'refreshfaqchannel':
							RefreshFaqChannel();
							break;
						case 'postseedmessage':
							CreateASeed();
							break;
						case 'postappealmessage':
							AppealMessage();
							break;
						case 'poststreamsmessage':
							StreamsMessage();
							break;
						case 'botcommandsmessage':
							CreateBotCommandsEmbed();
							break;
						case 'resetsignup':
							ClearChannel(racesignup);
							setTimeout(GetNextRaces, 10000, true, false);
							break;
					}
				}
			}
		}
	}
	catch (e)
	{
		console.log(e);
	}
});

client.on("guildMemberAdd", async member => {
	member.roles.add(restrictedid).catch(err => err);
	
	var discrim = member.user.discriminator;
	var discrimtext = "";
	var totaltime = Date.now() - member.joinedTimestamp;
	var invitelink = "UNKNOWN";
	
	if (discrim != "0") {
		discrimtext = "#" + discrim;
	}
		
	try
	{
		const cachedInvites = invites.get(member.guild.id);
		const newInvites = await member.guild.invites.fetch();
		const usedInvite = newInvites.find(inv => cachedInvites.get(inv.code) < inv.uses);
		
		invitelink = usedInvite.code;
	}
	catch (e)
	{
		console.log("Failed Adding User: " + e);
	}
		
	const sInfo = new EmbedBuilder()
		.setColor(8528115)
		.setTitle(`Member joined`)
		.setDescription(`User Created On: ${member.user.createdAt.toString()}\nUsed Invite: ${invitelink}`)
		.setAuthor({ name: GetDisplayNickname(member) + discrimtext + ` just joined`, iconURL: member.user.avatarURL() })
		.setFooter({ text: `User ID: ${member.id}` })
		.setTimestamp();
	  
	client.channels.cache.get(welcomemessagesid).send({ embeds: [sInfo] }).catch((err) => console.log(err));
})

client.on("guildMemberRemove", (member) => {
	try
	{
		var discrim = member.user.discriminator;
		var discrimtext = "";
		if (discrim != "0") {
			discrimtext = "#" + discrim;
		}
		
		let goodbyembed = new EmbedBuilder()
			.setColor("FF0000")
			.setAuthor({ name: GetDisplayNickname(member) + discrimtext + ` just left`, iconURL: member.user.avatarURL() });
		client.channels.cache.get(welcomemessagesid).send({ embeds: [goodbyembed] }).catch((err) => console.log(err));

		var racerstring = '&RacerName=' + GetDisplayNickname(member) + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + member.user.id;						
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");

		request.post(
			apilocation + 'UnregisterRacer?hash=' + hash + racerstring,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
				}
			}
		);
	}
	catch (e)
	{
		console.log("Failed Removing User: " + e);
	}
});

client.on('messageReactionAdd', (reaction, user) => {
	let message = reaction.message, emoji = reaction.emoji;
	
	if (message.id === restrictedmessageid) {
		var restrictedflag = '';
		
		switch (reaction.emoji.name) {
			case 'LazyKid':
				message.guild.members.fetch(user.id).then(member => {
					let currentdt = Math.floor(new Date().getTime());
					
					message.reactions.cache.find(reaction => reaction.emoji.name == "LazyKid").users.remove(user);
					
					if (currentdt - member.joinedTimestamp < 2000) {
						client.channels.cache.get(welcomemessagesid).send("Timer Kick");
						member.kick();
					} else {
						member.roles.remove(restrictedid);
					}
				});
				break;
			case 'boots':
				message.guild.members.fetch(user.id).then(member => {
					client.channels.cache.get(welcomemessagesid).send("Boots Kick");
					message.reactions.cache.find(reaction => reaction.emoji.name == "boots").users.remove(user);
					member.kick();
				});
				break;
		}
	}
});

//*****TRIGGERS END


//*****INTERACTIONS BEGIN

client.on("interactionCreate", async interaction => {
	var member = guild.members.cache.find(u => u.id === interaction.user.id);
	var name = GetDisplayNickname(member);
	
	//Bot Seed - #roll-a-seed
	if (interaction.customId == "selectmode") {
		interaction.deferReply({ephemeral: true});
		request.post(
			apilocation + 'BotSeed?hash=' + hash + '&flag_id=' + interaction.values[0],
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					var seedobj = JSON.parse(JSON.stringify(body));
					
					if (seedobj.valid == true) {
						var bodytext = 'Mode: ' + seedobj.FlagName;
						bodytext += '\n\nSeed: ' + seedobj.SeedURL;
						if (seedobj.SpoilerHash != null) {
							bodytext += '\n\nSpoiler: https://alttprladder.com/spoilers/getspoilerlog/' + seedobj.SpoilerHash;
						}
						bodytext += '\n\nSeed Hash: ' + GetSeedHash(seedobj.SeedHash);

						interaction.editReply({ embeds: [{ title: `Your seed is ready`, description: bodytext }], ephemeral: true }).catch(err => err);
					}
				}
			}
		);
    }
	
	//Register - #ladder-registration
    if (interaction.customId == "register") {
		var racerstring = '&RacerName=' + name + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + interaction.user.id;
		
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		request.post(
			apilocation + 'RegisterRacer?hash=' + hash + racerstring,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					var regobj = JSON.parse(JSON.stringify(body));
					
					if (regobj.valid == true) {
						//Add the role
						member.roles.add(racerrole).catch(err => err);
						interaction.reply({ embeds: [{ title: `Registration Complete`, description: `**` + name + `**, you have registered for the ALTTPR Ladder!\r\n\r\nYour username will display as **` + name + `** both here and on the website. If you wish to change it, please go to your server profile and set a nickname for the server.\r\n\r\nIf you have not done so already, please register your stream in ` + client.channels.cache.get(streamsid).toString() + `, as live streams are required to race in ALTTPR Ladder.\r\n\r\nHappy racing!` }],ephemeral: true }).catch(err => err);
						
						client.channels.cache.get(welcomemessagesid).send(name + " has registered").catch(err => err);	
					} else {
						interaction.reply({ embeds: [{ title: `Registration Incomplete`, description: name + `, you have already registered.` }], ephemeral: true }).catch(err => err);
					}
				} else {
				}
			}
		);
    }

    if (interaction.customId == "unregister") {
		var racerstring = '&RacerName=' + name + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + interaction.user.id;
		
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		request.post(
			apilocation + 'UnregisterRacer?hash=' + hash + racerstring,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					var regobj = JSON.parse(JSON.stringify(body));
					
					if (regobj.valid == true) {
						//Remove all role
						member.roles.remove(racerrole).catch(err => err);
						
						interaction.reply({ embeds: [{ title: `Unregistration Complete`, description: `**` + name + `**, you have unregistered from the ALTTPR Ladder.` }],ephemeral: true }).catch(err => err);

						client.channels.cache.get(welcomemessagesid).send(name + " has unregistered").catch(err => err);
					} else {
						interaction.reply({ embeds: [{ title: `Unregistration Incomplete`, description: name + `, you are not currently registered for the ALTTPR Ladder.` }], ephemeral: true }).catch(err => err);
					}
				} else {
				}
			}
		);
    }
	
    if (interaction.customId == "updatename") {
		var racerstring = '&RacerName=' + name + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + interaction.user.id;
		
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		request.post(
			apilocation + 'UpdateRacerName?hash=' + hash + racerstring,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					interaction.reply({ embeds: [{ title: `Name Update Complete`, description: `**` + name + `**, your display name has been updated on the ALTTPR website.` }],ephemeral: true }).catch(err => err);
				}
			}
		);
    }		
	
    if (interaction.customId == "openappeal") {
		const modal = new ModalBuilder()
			.setCustomId('appealsubmit')
			.setTitle('Submit Appeal');

		const detailsInput = new TextInputBuilder()
			.setCustomId('appealdetails')
			.setLabel("Please describe your appeal request")
			.setStyle(TextInputStyle.Paragraph);

		const row = new ActionRowBuilder().addComponents(detailsInput);

		modal.addComponents(row);

		interaction.showModal(modal);
    }
	
    if (interaction.customId == "appealsubmit") {
		interaction.reply({ embeds: [{ title: `Appeal Submitted`, description: `Thank you, your appeal has been sent to the Appeals Team and will be addressed as soon as they can review it. You will be contacted via DM.` }], ephemeral: true }).catch(err => err);

		client.channels.cache.get(appealrequestsid).send("<@&" + adminid + "> Appeal request from <@" + interaction.user.id + ">: " + interaction.fields.getTextInputValue('appealdetails')).catch(err => err);
    }
		
	if (interaction.customId == "setstream") {
		if (interaction.member.roles.cache.has(racerrole.id) == false) {
			interaction.reply({ embeds: [{ title: `Not Active Racer`, description: `You are not currently an active racer and cannot set your stream. Please register in: ` + client.channels.cache.get(ladderregistrationid).toString() }], ephemeral: true }).catch(err => err);
		}
		else {
			const modal = new ModalBuilder()
				.setCustomId('streamsubmit')
				.setTitle('Set Your Stream');

			const detailsInput = new TextInputBuilder()
				.setCustomId('streamurl')
				.setLabel("Please input your entire stream URL")
				.setStyle(TextInputStyle.Short);

			const row = new ActionRowBuilder().addComponents(detailsInput);

			modal.addComponents(row);

			interaction.showModal(modal);
		}
    }
	
	if (interaction.customId == "searchstream") {
		const modal = new ModalBuilder()
			.setCustomId('searchsubmit')
			.setTitle('Search for a Stream');

		const detailsInput = new TextInputBuilder()
			.setCustomId('searchname')
			.setLabel("Please input the username of the racer")
			.setStyle(TextInputStyle.Short);

		const row = new ActionRowBuilder().addComponents(detailsInput);

		modal.addComponents(row);

		interaction.showModal(modal);
    }
	
	if (interaction.customId == "streamsubmit") {
		var surl = interaction.fields.getTextInputValue('streamurl');
		var validstream = false;
		
		logMessage('streams', name, surl);
		
		if (surl.endsWith('/')) {
			surl = surl.substring(0, surl.length - 1);
		}
		
		if (surl.toLowerCase().indexOf('twitch.tv') > -1) {
			validstream = true;
			surl = "https://twitch.tv/" + surl.substring(surl.lastIndexOf('/') + 1).toLowerCase();
		} else if (surl.toLowerCase().indexOf('kick.com') > -1) {
			validstream = true;
			surl = "https://kick.com/" + surl.substring(surl.lastIndexOf('/') + 1).toLowerCase();
		} else if (surl.toLowerCase().indexOf('youtube.com') > -1) {
			validstream = true;
			surl = surl.toLowerCase();
		}
		
		if (validstream == false) {
			interaction.reply({ embeds: [{ title: `Invalid Stream`, description: `We were unable to validate your stream. Please confirm the full URL and try again. You can copy and paste in the URL from a browser to make it easy and accurate. We currently support Twitch, Kick, and YouTube streams. If you need additional support, please contact the admins.` }], ephemeral: true }).catch(err => err);
		} else {
			var racerstring = '&DiscordID=' + interaction.user.id + '&source=' + surl;
			
			racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
			
			request.post(
				apilocation + 'SetStream?hash=' + hash + racerstring,
				{ json: { key: 'value' } },
				function (error, response, body) {
					if (response.statusCode == 200) {
						interaction.reply({ embeds: [{ title: `Stream Updated`, description: `Thank you, your stream has been updated to: ` + surl }], ephemeral: true }).catch(err => err);
					}
				}
			);
		}
    }
	
	if (interaction.customId == "searchsubmit") {
		details = latinize(interaction.fields.getTextInputValue('searchname')).replace(/[^\x00-\x7F]/g, "").replace("#", "--");
		
		var resp = "";
		
		var racerstring = '&RacerName=' + details;
		request.post(
			apilocation + 'GetStream?hash=' + hash + racerstring,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					var racerobj = JSON.parse(JSON.stringify(body));
					
					if (racerobj.StreamList.length == 0) {
						resp = 'We were unable to locate any racer with this name';
					} else if (racerobj.StreamList.length == 1) {
						resp = 'Stream located at: ' + racerobj.StreamList[0].Stream;
					} else {
						resp = 'Multiple racers found with similar usernames. Please search again. We found racers with these usernames:\n\n';
						for (var i = 0; i < racerobj.StreamList.length; i++) {
							resp += racerobj.StreamList[i].Racer + '\n';
						}
					}
					
					interaction.reply({ embeds: [{ title: `Search Results`, description: resp }], ephemeral: true }).catch(err => err);
				}
			}
		);
    }
	
	if (interaction.customId == "joinrace") {
		var racerstring = '&RacerName=' + name + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + interaction.user.id;
					
		racerstring += '&RaceName=' + racename[signupschedule];

		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		JoinRace(member.user, name, racerstring, interaction);
    }
	
	if (interaction.customId == "leaverace") {
		if (leavecutoff == false) {
			var racerstring = '&RacerName=' + name + '&RacerLogin=' + GetRacerLogin(member) + '&DiscordID=' + interaction.user.id;
					
			racerstring += '&RaceName=' + racename[signupschedule];
			
			racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
			
			LeaveRace(member.user, name, racerstring, interaction);
		} else {
			interaction.deferUpdate();
		}

    }
	
	if (interaction.customId == "donerace") {
		var raceindex = 0;
		
		if (raceroom[0] != null)
		{
			if (interaction.message.channelId == raceroom[0].id) {
				raceindex = 0;
			}
		}
		
		if (raceroom[1] != null) {
			if (interaction.message.channelId == raceroom[1].id) {
				raceindex = 1;
			}
		}
		
		
		var racerstring = '&DiscordID=' + interaction.user.id + '&RaceName=' + racename[raceindex] + '&FinishTime=0';
					
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		FinishedRacing(0, raceindex, member.user, name, racerstring, interaction);
		
		const row = new ActionRowBuilder()
			.addComponents(
				new ButtonBuilder()
					.setCustomId('doneundo')
					.setLabel('Undo Finish')
					.setStyle(ButtonStyle.Danger),
				)	
				
		interaction.reply({ embeds: [{ title: `Race Completed`, description: "You have completed your race. If this was done by mistake, you have 10 seconds to undo your finish, or else it will be locked." }], components: [row], ephemeral: true }).catch(err => err);
    }

	if (interaction.customId == "doneundo") {
		var raceindex = 0;
		if (interaction.message.channelId == raceroom[0].id) {
			raceindex = 0;
		} else if (interaction.message.channelId == raceroom[1].id) {
			raceindex = 1;
		}
		
		var racerstring = '&DiscordID=' + interaction.user.id + '&RaceName=' + racename[raceindex];
					
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		UndoFinish(raceindex, member.user, name, racerstring, interaction);
    }

	if (interaction.customId == "ffrace") {
		const row = new ActionRowBuilder()
			.addComponents(
				new ButtonBuilder()
					.setCustomId('ffraceconfirm')
					.setLabel('Confirm Forfeit')
					.setStyle(ButtonStyle.Danger),
				)		
		
		interaction.reply({ embeds: [{ title: `Confirm Forfeit`, description: "Please confirm you want to forfeit. You cannot undo this action." }], components: [row], ephemeral: true }).catch(err => err);
    }
	
	if (interaction.customId == "ffraceconfirm") {
		var raceindex = 0;
		if (raceroom[0] != null)
		{
			if (interaction.message.channelId == raceroom[0].id) {
				raceindex = 0;
			}
		}
		
		if (raceroom[1] != null) {
			if (interaction.message.channelId == raceroom[1].id) {
				raceindex = 1;
			}
		}
	
		var racerstring = '&DiscordID=' + interaction.user.id + '&RaceName=' + racename[raceindex] + '&FinishTime=99999';
					
		racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
		
		FinishedRacing(99999, raceindex, member.user, name, racerstring, interaction);
		
		const row = new ActionRowBuilder()
			.addComponents(
				new ButtonBuilder()
					.setCustomId('doneundo')
					.setLabel('Undo Finish')
					.setStyle(ButtonStyle.Danger),
				)	
				
		interaction.deferUpdate();
		interaction.deleteReply();
    }
	
	if (interaction.customId == "commentrace") {
		const modal = new ModalBuilder()
			.setCustomId('commentsubmit')
			.setTitle('Submit Comment');

		const detailsInput = new TextInputBuilder()
			.setMaxLength(240)
			.setCustomId('commentdetails')
			.setLabel("Enter your comment (Limit 240 characters)")
			.setStyle(TextInputStyle.Short);

		const row = new ActionRowBuilder().addComponents(detailsInput);

		modal.addComponents(row);

		interaction.showModal(modal);
    }
	
    if (interaction.customId == "commentsubmit") {
		if (interaction.fields.getTextInputValue('commentdetails').length > 240) {
			interaction.reply({ embeds: [{ title: `Too many characters`, description: `Comments are limited to 240 characters in length` }], ephemeral: true }).catch(err => err);
		} else {
			//Submit comment
			var raceindex = 0;
			
			if (raceroom[0] != null)
			{
				if (interaction.message.channelId == raceroom[0].id) {
					raceindex = 0;
				}
			}
			
			if (raceroom[1] != null) {
				if (interaction.message.channelId == raceroom[1].id) {
					raceindex = 1;
				}
			}
			
			var racerstring = '&DiscordID=' + interaction.user.id + '&RaceName=' + racename[raceindex] + '&Comments=' + encodeURIComponent(interaction.fields.getTextInputValue('commentdetails'));
						
			racerstring = latinize(racerstring).replace(/[^\x00-\x7F]/g, "");
			
			SubmitComment(racerstring, interaction);
		}
    }
	
	if (interaction.customId == "racestatus") {
		interaction.deferUpdate();
		
		var texttosend = "";
		texttosend += "Variables:\nsignupschedule: " + signupschedule + "\n";
		texttosend += "racename: " + racename[0] + " - " + racename[1] + "\n";
		texttosend += "currentlyracing: " + currentlyracing[0] + " - " + currentlyracing[1] + "\n";
		texttosend += "signedup: " + signedup[0] + " - " + signedup[1] + "\n";
		texttosend += "racehours: " + racehours[0] + " - " + racehours[1] + "\n";
		texttosend += "spoilerhash: " + spoilerhash[0] + " - " + spoilerhash[1] + "\n";
		texttosend += "cutoff: " + leavecutoff + "\n";
		texttosend += "Check console for room details";
		
		guild.channels.cache.find(channel => channel.id === botcommandsid).send(texttosend);
		
		console.log("raceroom 1:");
		console.log(raceroom[0]);
		console.log("raceroom 2:");
		console.log(raceroom[1]);
		console.log("raceprerace 1:");
		console.log(raceprerace[0]);
		console.log("raceprerace 2:");
		console.log(raceprerace[1]);
		console.log("racelounge 1:");
		console.log(racelounge[0]);
		console.log("racelounge 2:");
		console.log(racelounge[1]);
		console.log("racerole 1:");
		console.log(racerole[0]);
		console.log("racerole 2:");
		console.log(racerole[1]);
		console.log("loungerole 1:");
		console.log(loungerole[0]);
		console.log("loungerole 2:");
		console.log(loungerole[1]);
    }	
	
	if (interaction.customId == "removeracer") {
		const modal = new ModalBuilder()
			.setCustomId('removeracersubmit')
			.setTitle('Remove Racer');

		const detailsInput = new TextInputBuilder()
			.setMaxLength(240)
			.setCustomId('removeracerdetails')
			.setLabel("Enter racer to remove from active race")
			.setStyle(TextInputStyle.Short);

		const row = new ActionRowBuilder().addComponents(detailsInput);

		modal.addComponents(row);

		interaction.showModal(modal);
    }
	
	if (interaction.customId == "removeracersubmit") {
		interaction.deferUpdate();
		guild.channels.cache.find(channel => channel.id === botcommandsid).send("NYI");
    }
	
	if (interaction.customId == "dqracer") {
		const modal = new ModalBuilder()
			.setCustomId('dqracersubmit')
			.setTitle('DQ Racer');

		const detailsInput = new TextInputBuilder()
			.setMaxLength(240)
			.setCustomId('dqracerdetails')
			.setLabel("Enter racer to DQ from active race")
			.setStyle(TextInputStyle.Short);

		const row = new ActionRowBuilder().addComponents(detailsInput);

		modal.addComponents(row);

		interaction.showModal(modal);
    }	
	
	if (interaction.customId == "dqracersubmit") {
		interaction.deferUpdate();
		guild.channels.cache.find(channel => channel.id === botcommandsid).send("NYI");
    }
	
	if (interaction.customId == "beginappeal") {
		activeappeal = true;
		interaction.deferUpdate();
		guild.channels.cache.find(channel => channel.id === botcommandsid).send("Appeal Opened");
    }	
	
	if (interaction.customId == "closeappeal") {
		activeappeal = false;
		interaction.deferUpdate();
		guild.channels.cache.find(channel => channel.id === botcommandsid).send("Appeal Closed");
    }	
	
	if (interaction.customId == "sendmessage") {
		const modal = new ModalBuilder()
			.setCustomId('sendmessagesubmit')
			.setTitle('Send Message Details');

		const detailsInput = new TextInputBuilder()
			.setMaxLength(240)
			.setCustomId('sendmessagechannel')
			.setLabel("Enter Channel")
			.setStyle(TextInputStyle.Short);

		const details2Input = new TextInputBuilder()
			.setCustomId('sendmessagemessage')
			.setLabel("Enter your message")
			.setStyle(TextInputStyle.Paragraph);
			
		const row = new ActionRowBuilder().addComponents(detailsInput);
		const row2 = new ActionRowBuilder().addComponents(details2Input);

		modal.addComponents(row, row2);

		interaction.showModal(modal);
    }	
	
	if (interaction.customId == "sendmessagesubmit") {
		interaction.deferUpdate();
		guild.channels.cache.find(channel => channel.name == interaction.fields.getTextInputValue('sendmessagechannel')).send(interaction.fields.getTextInputValue('sendmessagemessage'));
    }
})

//*****INTERACTIONS END



//*****RACE FUNCTIONS BEGIN


async function PrepareRace(raceroomname, schedule, mode, utcstartticks) {
	signupschedule = schedule;
	racename[schedule] = raceroomname;
	
	var r = guild.roles.cache.find(role => role.name == 'race-' + raceroomname);
	
	if (r == null) {
		var race_role = await guild.roles.create({
			name: 'race-' + raceroomname,
			color: '#FFFFFF'
		});
		
		racerole[schedule] = race_role;
		
		var lounge_role = await guild.roles.create({
			name: 'lounge-' + raceroomname,
			color: '#FFFFFF'
		});
		
		loungerole[schedule] = lounge_role;
	} else {
		racerole[schedule] = guild.roles.cache.find(role => role.name == 'race-' + raceroomname);
		loungerole[schedule] = guild.roles.cache.find(role => role.name == 'lounge-' + raceroomname);
	}

	var ch = guild.channels.cache.find(channel => channel.name == raceroomname + '-race');

	if (ch == null) {
		var race_room = await guild.channels.create({
			name: raceroomname + '-race',
			type: ChannelType.GuildText,
			parent: liveracescatid,
			permissionOverwrites: [
				{
					id: adminrole.id,
					allow: [PermissionFlagsBits.ViewChannel]
				},
				{
					id: racerole[schedule].id,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ReadMessageHistory],
					deny: [PermissionFlagsBits.SendMessages]
				},
				{
					id: lazykidid,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ManageRoles]
				},
				{
					id: guild.roles.everyone,
					deny: [PermissionFlagsBits.ViewChannel]
				}
			]
		});
		
		raceroom[schedule] = race_room;
		
		var prerace_room = await guild.channels.create({
			name: raceroomname + '-prerace',
			type: ChannelType.GuildText,
			parent: liveracescatid,
			permissionOverwrites: [
				{
					id: adminrole.id,
					allow: [PermissionFlagsBits.ViewChannel]
				},
				{
					id: racerole[schedule].id,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ReadMessageHistory, PermissionFlagsBits.SendMessages]
				},
				{
					id: lazykidid,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ManageRoles]
				},
				{
					id: guild.roles.everyone,
					deny: [PermissionFlagsBits.ViewChannel]
				}
			]
		});
		
		raceprerace[schedule] = prerace_room;
		
		var lounge_room = await guild.channels.create({
			name: raceroomname + '-lounge',
			type: ChannelType.GuildText,
			parent: liveracescatid,
			permissionOverwrites: [
				{
					id: adminrole.id,
					allow: [PermissionFlagsBits.ViewChannel]
				},
				{
					id: loungerole[schedule].id,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ReadMessageHistory, PermissionFlagsBits.SendMessages]
				},
				{
					id: lazykidid,
					allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ManageRoles]
				},
				{
					id: guild.roles.everyone,
					deny: [PermissionFlagsBits.ViewChannel]
				}
			]
		});
		
		racelounge[schedule] = lounge_room;
	} else {
		raceroom[schedule] = guild.channels.cache.find(channel => channel.name == raceroomname + '-race');
		raceprerace[schedule] = guild.channels.cache.find(channel => channel.name == raceroomname + '-prerace');
		racelounge[schedule] = guild.channels.cache.find(channel => channel.name == raceroomname + '-lounge');
	}
	
	var standingstext = '```';
	
	standingstext += '#' + raceroomname + ' - ' + mode + '\r\n';
	standingstext += 'Entrants: 0\r\n';
	standingstext += '========================================\r\n';
	standingstext += '```';
	
	var standings_msg1 = await raceroom[schedule].send(standingstext);
	var standings_msg2 = await raceroom[schedule].send('```========================================```');
	
	racestandings[0 + (schedule * 2)] = standings_msg1;
	racestandings[1 + (schedule * 2)] = standings_msg2;
	
	PostImage(raceprerace[schedule]);
	
	setTimeout(OpenRace, 15000, raceroomname, schedule, mode, utcstartticks);
}

async function OpenRace(raceroomname, schedule, mode, startutcticks) {
	var announcementrole = GetRacingRole(mode);
	var modemessage = 'Mode Definition: <' + GetModeDef(mode) + '>\n\n';
	
	racesignup.send("<@&" + racingannouncementsid + "> <@&" + announcementrole + ">\nSignups are open for race #" + raceroomname + " [" + mode + "], which starts at <t:" + ConvertToStartTime(startutcticks) + ":t>. Signups will remain open until <t:" + ConvertToSignupsClose(startutcticks) + ":t>.\n\n" + modemessage + "**=== SIGNUPS ARE NOW OPEN ===**");
	
	const embedd = new EmbedBuilder()
		.setTitle("Join or Leave Race")
		.setColor("#00FFFF")
		.setDescription("Click on Join Race to join this race, or Leave Race to leave. You will gain access to the race channels upon joining.");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('joinrace')
				.setLabel('Join Race')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('leaverace')
				.setLabel('Leave Race')
				.setStyle(ButtonStyle.Primary),
			)

	racesignup.send({embeds: [embedd], components: [row] });

	//PROPER TIME
	//setTimeout(CloseSignups, 1680000, raceroomname, schedule);
	setTimeout(CutoffSignups, 1200000, raceroomname, schedule);
	
	//TEST TIME
	//setTimeout(CloseSignups, 30000, raceroomname, schedule);
}

function CutoffSignups(raceroomname, schedule) {
	if (signedup[schedule] > 1) {
		racesignup.send("**=== THE SEED HAS BEEN ROLLED AND YOU ARE NOW UNABLE TO LEAVE THIS RACE. YOU CAN STILL SIGN UP FOR 8 MINUTES, BUT WILL BE UNABLE TO LEAVE. ===**");
		leavecutoff = true;
	
		raceroom[schedule].send('**This race will begin in 10 minutes**\n\nRace seed is being generated, please wait...\r\n\r\nThis process is usually fast, but could take up to 2-3 minutes. If no seed is available after 5 minutes, please alert the admins.');
		
		GenerateSeed(raceroomname, schedule);
		
		//PROPER TIME
		setTimeout(StartCountdown, 300000, schedule);
		setTimeout(PrepareCleanup, 480000);
		//TEST TIME
		//setTimeout(StartCountdown, 30000, schedule);
	} else {
		CancelRace(schedule);
		PrepareCleanup();
	}
}

function PrepareCleanup() {
	leavecutoff = false;
	
	ClearChannel(racesignup);

	setTimeout(GetNextRaces, 10000, true, false);
}

function CloseSignups(raceroomname, schedule) {
	ClearChannel(racesignup);

	setTimeout(GetNextRaces, 10000, true, false);
	
	if (signedup[schedule] > 1) {
		raceroom[schedule].send('**This race will begin in 10 minutes**\n\nRace seed is being generated, please wait...\r\n\r\nThis process is usually fast, but could take up to 2-3 minutes. If no seed is available after 5 minutes, please alert the admins.');
		
		GenerateSeed(raceroomname, schedule);
		
		//PROPER TIME
		setTimeout(StartCountdown, 300000, schedule);
		//TEST TIME
		//setTimeout(StartCountdown, 30000, schedule);
	} else {
		CancelRace(schedule);
	}
	
	
}

function GenerateSeed(raceroomname, schedule) {
	request.post(
		apilocation + 'CloseSignups?hash=' + hash + '&RaceName=' + raceroomname,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var seedobj = JSON.parse(JSON.stringify(body));
				
				var grabbagmode = '';
				var grabbagmessage = '';
				
				if (seedobj.valid == true) {
					if (seedobj.GrabBag != null) {
						grabbagmessage = '\n\nThis race\'s mode is: **' + seedobj.GrabBag + '**';
					} else {
						grabbagmessage = '';
					}
					
					raceroom[schedule].send('<@&' + racerole[schedule].id + '>');
					
					var messagetosend = '**You can download your race seed here:**\n\n' + seedobj.SeedURL;
					
					if (seedobj.SpoilerHash != null) {
						spoilerhash[schedule] = seedobj.SpoilerHash;
						messagetosend += '\n\nYour spoiler log will be posted here in 10 minutes and then you will have 15 minutes to review your log before the race begins';
					}
					
					messagetosend += '\n\nSeed Hash: ' + GetSeedHash(seedobj.SeedHash) + '\n\nMode Definition: <' + GetModeDef(seedobj.FlagName) + '>';
					
					messagetosend += grabbagmessage;
					
					messagetosend += '\n\nView Live Standings: https://alttprladder.com/home/loadstandings/' + raceroomname;
					
					const exampleEmbed = new EmbedBuilder()
						.setColor('#FFFFFF')
						.setTitle('Race Details')
						.setDescription(messagetosend)
						.setTimestamp();
						
					raceroom[schedule].send({ embeds: [exampleEmbed] });
					
					//raceroom[schedule].send(messagetosend);
					
					racehours[schedule] = seedobj.Hours;
				} else {
					client.channels.cache.get(botcommandsid).send("<@219930246097534976>\n**There was an issue generating the seed**");
				}
			}
		}
	);
}


function StartCountdown(schedule) {
	if (spoilerhash[schedule] == '') {
		raceroom[schedule].send('**This race will begin in 5 minutes. If your seed has not yet been posted, please alert the admins.**');
	} else {
		raceroom[schedule].send('**The spoiler log review period will begin in 5 minutes**');
	}
	
	//PROPER TIME
	setTimeout(StartCountdown1m, 240000, schedule);
	
	//TEST TIME
	//setTimeout(StartCountdown1m, 24000, schedule);
}

function StartCountdown1m(schedule) {
	if (spoilerhash[schedule] == '') {
		raceroom[schedule].send('<@&' + racerole[schedule].id + '> **This race will begin in 1 minute\n\nA reminder, make sure the credits play out until the \'The End\' screen with your collection rates and in-game Time!**');
	} else {
		raceroom[schedule].send('<@&' + racerole[schedule].id + '> **The spoiler log review period will begin in 1 minute, at which time, your spoiler log will be posted here.**');
	}
	
	setTimeout(StartCountdown30s, 30000, schedule);
}

function StartCountdown30s(schedule) {
	if (spoilerhash[schedule] == '') {
		raceroom[schedule].send('**This race will begin in 30 seconds**');
	} else {
		raceroom[schedule].send('**The spoiler log review period will begin in 30 seconds**');
	}
	
	setTimeout(StartCountdown10s, 20000, schedule);
}

function StartCountdown10s(schedule) {
	if (spoilerhash[schedule] == '') {
		raceroom[schedule].send('**This race will begin in 10 seconds**');
	} else {
		raceroom[schedule].send('**The spoiler log review period will begin in 10 seconds**');
	}
	
	setTimeout(StartCountdown3s, 7000, schedule);
}

function StartCountdown3s(schedule) {
	raceroom[schedule].send("**3**");
	
	setTimeout(StartCountdown2s, 1000, schedule);
}

function StartCountdown2s(schedule) {
	raceroom[schedule].send("**2**");
	
	setTimeout(StartCountdown1s, 1000, schedule);
}

function StartCountdown1s(schedule) {
	raceroom[schedule].send("**1**");
	
	setTimeout(StartRace, 1000, schedule);
}

function StartRace(schedule) {
	if (spoilerhash[schedule] == '') {
		raceroom[schedule].send('**GO!**\n\nYou will be given access to the Race Lounge when you are finished\n\nYou have ' + racehours[schedule] + ' hours to finish this race');
		FinishRaceMessage(schedule);
	} else {
		SendSpoilerStart(schedule);
		setTimeout(StartSpoilerCountdown, 300000, schedule);
	}
	
	request.post(
		apilocation + 'StartRace?hash=' + hash + '&RaceName=' + racename[schedule],
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
			}
		}
	);
	
	//setTimeout(EndRaceWithoutAllComplete, 60000, schedule);
	
	setTimeout(EndRaceWithoutAllComplete, 3600000 * racehours[schedule], schedule);
}

function SendSpoilerStart(schedule) {
	const exampleEmbed = new EmbedBuilder()
		.setColor('#FF00FF')
		.setTitle("Spoiler Log Download")
		.setDescription('**GO!**\n\n** === THE SPOILER LOG REVIEW PERIOD HAS BEGUN! === **\n\nYou may now review your spoiler log, which is available to download here or at:\n\n https://alttprladder.com/spoilers/getspoilerlog/' +  spoilerhash[schedule] + '\n\nYou have 15 minutes to review the log. You will get a new countdown here for when you can begin racing.');

	raceroom[schedule].send({files: ['C:\\ALTTPRSpoilerLogs\\' + spoilerhash[schedule] + '.txt']});
	
	raceroom[schedule].send({ embeds: [exampleEmbed] }).catch((err) => console.log(err));
}

function StartSpoilerCountdown(schedule) {
	raceroom[schedule].send('**You have 10 minutes left to review your spoiler log**');
	
	setTimeout(StartSpoilerCountdown5m, 300000, schedule);
}

function StartSpoilerCountdown5m(schedule) {
	raceroom[schedule].send('**You have 5 minutes left to review your spoiler log**');
	
	setTimeout(StartSpoilerCountdown1m, 240000, schedule);
}

function StartSpoilerCountdown1m(schedule) {
	raceroom[schedule].send('<@&' + racerole[schedule].id + '> **You have 1 minute left to review your spoiler log, and then you can begin racing\n\nA reminder, make sure the credits play out until the \'The End\' screen with your collection rates and in-game Time!**');
	
	setTimeout(StartSpoilerCountdown30s, 30000, schedule);
}

function StartSpoilerCountdown30s(schedule) {
	raceroom[schedule].send('**You may begin racing in 30 seconds**');
	
	setTimeout(StartSpoilerCountdown10s, 20000, schedule);
}

function StartSpoilerCountdown10s(schedule) {
	raceroom[schedule].send('**You may begin racing in 10 seconds**');
	
	setTimeout(StartSpoilerCountdown3s, 7000, schedule);
}

function StartSpoilerCountdown3s(schedule) {
	raceroom[schedule].send('**3**');
	
	setTimeout(StartSpoilerCountdown2s, 1000, schedule);
}

function StartSpoilerCountdown2s(schedule) {
	raceroom[schedule].send('**2**');
	
	setTimeout(StartSpoilerCountdown1s, 1000, schedule);
}

function StartSpoilerCountdown1s(schedule) {
	raceroom[schedule].send('**1**');
	
	setTimeout(StartSpoilerRace, 1000, schedule);
}

function StartSpoilerRace(schedule) {
	raceroom[schedule].send('**GO!**\n\nYou will be given access to the Race Lounge when you are finished\n\nYou have 1 hour and 45 minutes to finish this race');
	
	FinishRaceMessage(schedule);
}

function EndRaceWithoutAllComplete(schedule) {
	if (racehours[schedule] != 0 && currentlyracing[schedule] > 0) {
		request.post(
			apilocation + 'AutoFF?hash=' + hash + '&RaceName=' + racename[schedule],
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					RaceHasFinished(schedule, 20);
					
					var raceobj = JSON.parse(JSON.stringify(body));
					
					for (var i = 0; i < raceobj.DiscordID.length; i++) {
						var member = guild.members.cache.find(u => u.id === raceobj.DiscordID[i]);
						var name = GetDisplayNickname(member);						
						
						member.roles.add(loungerole[schedule]).catch(err => err);
						
						racelounge[schedule].send('**' + name + '** has forfeited\n').catch(err => err);
						
						client.users.cache.get(raceobj.DiscordID[i]).send('The ALTTPR Ladder race has concluded and your race time has been entered as a forfeit. If you wish to appeal this result, please appeal via the #appeals channel, thank you!').catch((err) => console.log(err));
					}
				}
			}
		);
	}
}

function CancelRace(schedule) {
	request.post(
		apilocation + 'CancelRace?hash=' + hash + '&RaceName=' + racename[schedule],
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
			}
		}
	);
	
	CloseRace(schedule);
}

function CloseRace(schedule) {
	leavecutoff = false;
	racename[schedule] = '';
	spoilerhash[schedule] = '';
	racehours[schedule] = 0;
	raceroom[schedule].delete().then(raceroom[schedule] = null);
	raceprerace[schedule].delete().then(raceprerace[schedule] = null);
	racelounge[schedule].delete().then(racelounge[schedule] = null);
	racerole[schedule].delete().then(racerole[schedule] = null);
	loungerole[schedule].delete().then(loungerole[schedule] = null);
	currentlyracing[schedule] = 0;
	signedup[schedule] = 0;
	racestandings[schedule * 2] = null;
	racestandings[1 + (schedule * 2)] = null;
}

function ClearRace(raceroom) {
	guild.channels.cache.find(channel => channel.name == raceroom + '-race').delete().catch(err => err);
	guild.channels.cache.find(channel => channel.name == raceroom + '-prerace').delete().catch(err => err);
	guild.channels.cache.find(channel => channel.name == raceroom + '-lounge').delete().catch(err => err);
	guild.roles.cache.find(role => role.name == 'race-' + raceroom).delete().catch(err => err);
	guild.roles.cache.find(role => role.name == 'lounge-' + raceroom).delete().catch(err => err);
}

async function JoinRace(user, name, racerstring, interaction) {
	request.post(
		apilocation + 'JoinRace?hash=' + hash + racerstring,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					UpdateRaceMessages(raceobj, signupschedule);
					racesignup.send("**" + name + "**, you have entered this race! *[" + raceobj.RacerCount + " Entered]*\n").catch(err => err);
					interaction.deferUpdate();
					
					interaction.member.roles.add(racerole[signupschedule]).catch(err => err);
					
					currentlyracing[signupschedule] = raceobj.RacerCount;
					signedup[signupschedule] = raceobj.RacerCount;
				} else {
					if (raceobj.Reason == 'Racer Does Not Exist') {
						interaction.reply({ embeds: [{ title: `Not Registered`, description: `You are currently not registered in Ladder` }],ephemeral: true }).catch(err => err);
					} else if (raceobj.Reason == 'Race Does Not Exist') {
						console.log(raceobj.Reason);
					} else if (raceobj.Reason == 'No Stream') {
						interaction.reply({ embeds: [{ title: `No Stream Set`, description: `You do not have a stream set. Please set your stream at ` + client.channels.cache.get(streamsid).toString() + ` and then join the race.` }],ephemeral: true }).catch(err => err);
					} else if (raceobj.Reason == 'Racer Already Entered') {
						interaction.reply({ embeds: [{ title: `Already Entered`, description: `You are already entered in this race` }],ephemeral: true }).catch(err => err);
					} else {
						interaction.reply({ embeds: [{ title: `In Another Race`, description: `You are actively racing in another race and cannot join another until you finish or forefit the active race` }],ephemeral: true }).catch(err => err);
					}
				}
			} else {
				console.log('Invalid Response');
				interaction.reply({ embeds: [{ title: `Issue Joining Race`, description: `There was an issue joining this race. Please try again, or contact an admin.` }],ephemeral: true }).catch(err => err);
			}
		}
	);
}

async function LeaveRace(user, name, racerstring, interaction) {
	request.post(
		apilocation + 'LeaveRace?hash=' + hash + racerstring,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					UpdateRaceMessages(raceobj, signupschedule);
					racesignup.send("**" + name + "**, you have left this race *[" + raceobj.RacerCount + " Entered]*\n").catch(err => err);
					interaction.deferUpdate();
					
					interaction.member.roles.remove(racerole[signupschedule]).catch(err => err);
					
					currentlyracing[signupschedule] = raceobj.RacerCount;
					signedup[signupschedule] = raceobj.RacerCount;
				} else {
					if (raceobj.Reason == 'Racer Does Not Exist') {
						console.log(raceobj.Reason);
					} else if (raceobj.Reason == 'Race Does Not Exist') {
						console.log(raceobj.Reason);
					} else if (raceobj.Reason == 'Racer Has Not Joined') {
						interaction.reply({ embeds: [{ title: `Not In Race`, description: `You are not entered in this race` }],ephemeral: true }).catch(err => err);
					} else {
						console.log(raceobj.Reason);
					}
					resp = raceobj.Reason;
				}
			} else {
				console.log('Invalid Response');
				interaction.reply({ embeds: [{ title: `Issue Leaving Race`, description: `There was an issue leaving this race. Please try again, or contact an admin.` }],ephemeral: true }).catch(err => err);
			}
		}
	);
}

async function GetRaceSignups(name, raceindex) {
	request.post(
		apilocation + 'GetRaceSignupMessage?hash=' + hash + '&RaceName=' + name,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					UpdateRaceMessages(raceobj, raceindex);
				}
			}
		}
	);
}

function UpdateRaceMessages(raceobj, raceindex) {
	//console.log(raceobj);
	var postdivider = false;
	
	var standingstext = '```';
	standingstext += raceobj.RaceName + ' - ' + raceobj.FlagName + '\r\n';
	standingstext += 'Entrants: ' + raceobj.RacerCount + ' - Racing: ' + raceobj.CurrentlyRacing + '\r\n';
	standingstext += '========================================\r\n';

	for (var x = 0; x < 50; x++) {
		if (raceobj.RacerStandingList.length > x) {
			if (raceobj.RacerStandingList[x].FinishTime != 0) {
				standingstext += raceobj.RacerStandingList[x].StandingFinish + ') ' + raceobj.RacerStandingList[x].RacerName + '  ' + raceobj.RacerStandingList[x].RaceTime + '\r\n';
			} else {
				if (postdivider === false) {
					standingstext += '\r\n===== ACTIVE RACERS =====\r\n';
					postdivider = true;
				}
				standingstext += raceobj.RacerStandingList[x].RacerName + ' - [' + raceobj.RacerStandingList[x].Ranking + ']\r\n';
			}
		}
	}
	
	standingstext += '```';
	
	racestandings[0 + (raceindex * 2)].edit(standingstext);
		
	if (raceobj.RacerCount > 50) {
		standingstext = '```';
			for (var x = 50; x < raceobj.RacerCount; x++) {
				if (raceobj.RacerStandingList.length > x) {
					if (raceobj.RacerStandingList[x].FinishTime != 0) {
						standingstext += raceobj.RacerStandingList[x].StandingFinish + ') ' + raceobj.RacerStandingList[x].RacerName + '  ' + raceobj.RacerStandingList[x].RaceTime + '\r\n';
					} else {
						if (postdivider === false) {
							standingstext += '\r\n===== ACTIVE RACERS =====\r\n';
							postdivider = true;
						}
						standingstext += raceobj.RacerStandingList[x].RacerName + ' - [' + raceobj.RacerStandingList[x].Ranking + ']\r\n';
					}
				}
			}
		standingstext += '```';
		
		racestandings[1 + (raceindex * 2)].edit(standingstext);
	}
}

function GetNextRaces(update, timers) {
	request.post(
		apilocation + 'GetNextRaces?hash=' + hash + '&NumberOfRaces=10',
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var scheduleobj = JSON.parse(JSON.stringify(body));
				
				if (update == true) {
					var racedesc = '';
					var currentraces = '\n\n**Active Races**\n';
					
					for (var i = 0; i < scheduleobj.ScheduleDetails.length; i++) {
						if (scheduleobj.ScheduleDetails[i].RaceStatus == 0) {
							racedesc += '<t:' + scheduleobj.ScheduleDetails[i].UTCStart + ':f> [' + scheduleobj.ScheduleDetails[i].ModeName + ']';
							if (scheduleobj.ScheduleDetails[i].IsChampionshipRace == true) {
								racedesc += ' (C)';
							}
							
							racedesc += '\n';
						} else {
							currentraces += 'https://alttprladder.com/schedule/racedetails/' + scheduleobj.ScheduleDetails[i].RaceName + ' [' + scheduleobj.ScheduleDetails[i].ModeName + ']\n';
						}
					}
					
					const exampleEmbed = new EmbedBuilder()
						.setColor('#FFFFFF')
						.setTitle('Next Scheduled Races (All times local)')
						.setDescription(racedesc + currentraces + '\nSignups begin 30 minutes before the start of the race and will remain open for 28 minutes. The seed will be rolled 10 minutes before the start of the race and you will be unable to leave the race after the seed has been rolled. Announcements will be made when the signups are opened.\n\nWhen you sign up, you will be given access to the racing channels. When signups close, your seed will be posted in the race channel with a countdown on when to start.\n\nPlease visit https://alttprladder.com/schedule for the entire schedule.')
						.setTimestamp()

					client.channels.cache.get(racesignupid).send({ embeds: [exampleEmbed] });
				}
				
				if (timers == true) {
					timerneedsreset = true;
					
					if (scheduleobj.ScheduleDetails.length > 0) {
						if (racename[scheduleobj.ScheduleDetails[0].Schedule] == '') {
							SetRaceStatus(scheduleobj.ScheduleDetails[0].Schedule, scheduleobj.ScheduleDetails[0]);
						}
						
						if (scheduleobj.ScheduleDetails.length > 1) {
								if (racename[scheduleobj.ScheduleDetails[1].Schedule] == '' && scheduleobj.ScheduleDetails.length > 1 && scheduleobj.ScheduleDetails[0].RaceStatus == 3) {
								SetRaceStatus(scheduleobj.ScheduleDetails[1].Schedule, scheduleobj.ScheduleDetails[1]);
							}
						}
					}

					
					if (timerneedsreset === true) {
						//console.log("Timer 10m: " + new Date().toLocaleString());
						setTimeout(GetNextRaces, 600000, false, true);
						timerneedsreset = false;
					}
				}
			}
		}
	);	
}

async function SetRaceStatus(schedule, scheduleobject) {
	//console.log(scheduleobject);
	
	if (scheduleobject == null)
	{
		return;
	}
	
	//RESET TIMERS
	//0: Scheduled
	//1: Signups
	//2: Closed
	//3: Racing
	//4: Ended
	//If first is 0, no races are active
	if (scheduleobject.RaceStatus == 0) {
		//If greater than 5 minutes, reset timer and do it again in 5
		if (scheduleobject.UTCTimerTicks <= 600) {
			var startticks = (scheduleobject.UTCTimerTicks - 15) * 1000;
			if (startticks < 0) {
				startticks = 1000;
			}
			
			setTimeout(PrepareRace, startticks, scheduleobject.RaceName, scheduleobject.Schedule, scheduleobject.ModeName, scheduleobject.UTCStart);
			
			//A race is set to begin, set the timeout to be 40 minutes from the start of signups and don't reset afterward
			timerneedsreset = false;
			setTimeout(GetNextRaces, 2400000, false, true);
		}
	//If first is 1, race is in signup
	} else if (scheduleobject.RaceStatus == 1) {
		signupschedule = schedule;
		racename[schedule] = scheduleobject.RaceName;
		racerole[schedule] = guild.roles.cache.find(role => role.name == 'race-' + scheduleobject.RaceName);
		loungerole[schedule] = guild.roles.cache.find(role => role.name == 'lounge-' + scheduleobject.RaceName);
		raceroom[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-race');
		raceprerace[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-prerace');
		racelounge[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-lounge');
		currentlyracing[schedule] = scheduleobject.ActivelyRacing;
		signedup[schedule] = scheduleobject.CurrentlySignedUp;

		var standingstext = '```';
		
		standingstext += '#' + scheduleobject.RaceName + ' - ' + scheduleobject.ModeName + '\r\n';
		standingstext += 'Entrants: ' + scheduleobject.CurrentlySignedUp.toString() + '\r\n';
		standingstext += '========================================\r\n';
		standingstext += '```';
		
		var standings_msg1 = await raceroom[schedule].send(standingstext);
		var standings_msg2 = await raceroom[schedule].send('```========================================```');
		
		racestandings[0 + (schedule * 2)] = standings_msg1;
		racestandings[1 + (schedule * 2)] = standings_msg2;
		
		GetRaceSignups(racename[schedule], schedule);
		
		setTimeout(CloseSignups, scheduleobject.UTCTimerTicks * 1000, racename[schedule], signupschedule);
	//If first is 2, race has closed signups, trigger close event to see if race is valid
	} else if (scheduleobject.RaceStatus == 2) {
		signupschedule = schedule;
		racename[schedule] = scheduleobject.RaceName;
		racerole[schedule] = guild.roles.cache.find(role => role.name == 'race-' + scheduleobject.RaceName);
		loungerole[schedule] = guild.roles.cache.find(role => role.name == 'lounge-' + scheduleobject.RaceName);
		raceroom[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-race');
		raceprerace[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-prerace');
		racelounge[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-lounge');
		currentlyracing[schedule] = scheduleobject.ActivelyRacing;
		signedup[schedule] = scheduleobject.CurrentlySignedUp;
		
		racehours[schedule] = scheduleobject.Hours;
		spoilerhash[schedule] = scheduleobject.SpoilerHash;
		
		var standingstext = '```';
		
		standingstext += '#' + scheduleobject.RaceName + ' - ' + scheduleobject.ModeName + '\r\n';
		standingstext += 'Entrants: ' + scheduleobject.CurrentlySignedUp.toString() + '\r\n';
		standingstext += '========================================\r\n';
		standingstext += '```';
		
		var standings_msg1 = await raceroom[schedule].send(standingstext);
		var standings_msg2 = await raceroom[schedule].send('```========================================```');
		
		racestandings[0 + (schedule * 2)] = standings_msg1;
		racestandings[1 + (schedule * 2)] = standings_msg2;
		
		GetRaceSignups(racename[schedule], schedule);
		
		//If the seed has not yet been rolled, roll the seed
		if (scheduleobject.SeedURL == null) {
			GenerateSeed(racename[schedule], schedule);
		}
		
		if (scheduleobject.UTCTimerTicks > 300) {
			setTimeout(StartCountdown, (scheduleobject.UTCTimerTicks - 300) * 1000, 0);
		} else if (scheduleobject.UTCTimerTicks > 60) {
			setTimeout(StartCountdown1m, scheduleobject.UTCTimerTicks * 1000, 0);
		} else {
			StartCountdown1m(schedule);
		}
	//If first is 3, check second to see if that race should begin its process
	} else if (scheduleobject.RaceStatus == 3) {
		racename[schedule] = scheduleobject.RaceName;
		racerole[schedule] = guild.roles.cache.find(role => role.name == 'race-' + scheduleobject.RaceName);
		loungerole[schedule] = guild.roles.cache.find(role => role.name == 'lounge-' + scheduleobject.RaceName);
		raceroom[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-race');
		raceprerace[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-prerace');
		racelounge[schedule] = guild.channels.cache.find(channel => channel.name == scheduleobject.RaceName + '-lounge');
		currentlyracing[schedule] = scheduleobject.ActivelyRacing;
		signedup[schedule] = scheduleobject.CurrentlySignedUp;
		
		racehours[schedule] = scheduleobject.Hours;
		spoilerhash[schedule] = scheduleobject.SpoilerHash;
		
		var standingstext = '```';
		
		standingstext += '#' + scheduleobject.RaceName + ' - ' + scheduleobject.ModeName + '\r\n';
		standingstext += 'Entrants: ' + scheduleobject.CurrentlySignedUp.toString() + '\r\n';
		standingstext += '========================================\r\n';
		standingstext += '```';
		
		var standings_msg1 = await raceroom[schedule].send(standingstext);
		var standings_msg2 = await raceroom[schedule].send('```========================================```');
		
		racestandings[0 + (schedule * 2)] = standings_msg1;
		racestandings[1 + (schedule * 2)] = standings_msg2;
		
		GetRaceSignups(racename[schedule], schedule);
		
		setTimeout(EndRaceWithoutAllComplete, scheduleobject.UTCTimerTicks * 1000, schedule);
	}
}


async function FinishedRacing(time, raceindex, user, name, racerstring, interaction) {
	request.post(
		apilocation + 'RacerDone?hash=' + hash + racerstring,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					UpdateRaceMessages(raceobj, raceindex);
					
					interaction.member.roles.add(loungerole[raceindex]).catch(err => err);
					
					if (time == 0) {
						racelounge[raceindex].send('**' + name + '** has finished in ' + raceobj.Finish + ' place with a time of ' + raceobj.FinishTime + '\n').catch(err => err);
					} else {
						racelounge[raceindex].send('**' + name + '** has forfeited\n').catch(err => err);
					}
					
					currentlyracing[raceindex] = raceobj.CurrentlyRacing;
					signedup[raceindex] = raceobj.RacerCount;
					
					if (raceobj.CurrentlyRacing == 0) {
						//Trigger finished racing function
						RaceHasFinished(raceindex, 20);
					}
				} else {
					if (raceobj.Reason == 'Racer Does Not Exist') {
						console.log(raceobj.Reason);
					} else if (raceobj.Reason == 'Race Does Not Exist') {
						console.log(raceobj.Reason);
					} else if (raceobj.Reason == 'Racer Already Finished') {
						//console.log('Finish:' + raceobj.Reason);
					} else {
						console.log('Finish2:' + raceobj.Reason);
					}
				}
			} else {
				console.log('Invalid Response');
				interaction.reply({ embeds: [{ title: `Issue Finishing Race`, description: `There was an issue finishing this race. Please try again, or contact an admin.` }],ephemeral: true }).catch(err => err);
			}
		}
	);
}

async function UndoFinish(raceindex, user, name, racerstring, interaction) {
	request.post(
		apilocation + 'UndoFinish?hash=' + hash + racerstring,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					UpdateRaceMessages(raceobj, raceindex);
					
					interaction.member.roles.remove(loungerole[raceindex]).catch(err => err);
					
					racelounge[raceindex].send('**' + name + '** has undone their finish\n').catch(err => err);
					
					currentlyracing[raceindex] = raceobj.CurrentlyRacing;
					signedup[raceindex] = raceobj.RacerCount;
					
					interaction.reply({ embeds: [{ title: `Finish Undone`, description: `You have undone your finish, you may continue racing` }],ephemeral: true }).catch(err => err);
				} else {
					if (raceobj.Reason == 'Time Elapsed') {
						interaction.reply({ embeds: [{ title: `Unable To Undo`, description: `The 10 second window to undo your finish has passed. If you need to adjust your time, please put in an Appeal request.` }],ephemeral: true }).catch(err => err);
					} else if (raceobj.Reason == 'Race Does Not Exist') {
						interaction.reply({ embeds: [{ title: `Unable To Undo`, description: `We are unable to undo your finish due to the race being completed.` }],ephemeral: true }).catch(err => err);
					} else if (raceobj.Reason == 'Racer Already Finished') {
						console.log('Undo:' + raceobj.Reason);
					} else {
						console.log('Undo2:' + raceobj.Reason);
					}
				}
			} else {
				console.log('Invalid Response');
				interaction.reply({ embeds: [{ title: `Issue Finishing Race`, description: `There was an issue finishing this race. Please try again, or contact an admin.` }],ephemeral: true }).catch(err => err);
			}
		}
	);
}

async function SubmitComment(racerstring, interaction) {
	request.post(
		apilocation + 'AddComment?hash=' + hash + racerstring,
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				var raceobj = JSON.parse(JSON.stringify(body));
				if (raceobj.valid == true) {
					interaction.reply({ embeds: [{ title: `Comment Submitted`, description: `Comment Submitted` }],ephemeral: true }).catch(err => err);
				} else {
					interaction.reply({ embeds: [{ title: `Comment Submitted`, description: `Unable to submit comment while you are actively racing` }],ephemeral: true }).catch(err => err);
				}
			} else {
				interaction.reply({ embeds: [{ title: `Comment Submitted`, description: `Unable to submit comment. Comments are limited to 240 characters.` }],ephemeral: true }).catch(err => err);
			}
		}
	);
}

function RaceHasFinished(raceindex, timer) {
	request.post(
		apilocation + 'CompleteRace?hash=' + hash + '&RaceName=' + racename[raceindex],
		{ json: { key: 'value' } },
		function (error, response, body) {
			if (response.statusCode == 200) {
				CalculateRankings();
			}
		}
	);
	
	var ticks = (timer - 2) * 60000;
	
	const exampleEmbed = new EmbedBuilder()
		.setColor('#FF0000')
		.setTitle('Race Concluded')
		.setDescription('This race has concluded! If you think your race finished time is wrong, you wish to appeal, or you wish for your race to be investigated, you have six hours from now to submit an appeal. If you do not appeal, the rankings will be locked. If no admin is available before the period closes, any corrections to the rankings will take place after the appeal.\n\nThe Racing Lounge will remain open for 20 minutes and then close.\n\nView the race results and ratings change here: https://alttprladder.com/schedule/racedetails/' + racename[raceindex])
		.setTimestamp();

	racelounge[raceindex].send({ embeds: [exampleEmbed] }).catch((err) => console.log(err));
	
	setTimeout(TwoMinuteWarning, ticks, raceindex);
}

function TwoMinuteWarning(raceindex) {
	racelounge[raceindex].send("You don't have to go home, but you can't stay here. The Lounge will close in 2 minutes.");
	setTimeout(CloseRace, 120000, raceindex);
}

function CalculateRankings() {
	if (activeappeal === false) {
		request.post(
			apilocation + 'CalculateRace?hash=' + hash,
			{ json: { key: 'value' } },
			function (error, response, body) {
				if (response.statusCode == 200) {
					var calcobj = JSON.parse(JSON.stringify(body));
					
					if (calcobj.valid == false) {
						client.channels.cache.get(botcommandsid).send("<@219930246097534976>\n**There was an issue calculating rankings**");
					}
				} else {
					client.channels.cache.get(botcommandsid).send("<@219930246097534976>\n**Calculate Race Failure**");
				}
			}
		);
	}
}  

function FinishRaceMessage(schedule) {
	const embedd = new EmbedBuilder()
		.setTitle("Race has started")
		.setColor("#00FFFF")
		.setDescription("Click on Done to finish, or Forfeit to quit. When you are done, you can also add a comment.");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('donerace')
				.setLabel('Done')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('ffrace')
				.setLabel('Forfeit')
				.setStyle(ButtonStyle.Danger),
			new ButtonBuilder()
				.setCustomId('commentrace')
				.setLabel('Comment')
				.setStyle(ButtonStyle.Success),
			);
	
	raceroom[schedule].send({embeds: [embedd], components: [row] });
	
	raceprerace[schedule].permissionOverwrites.set([
		{
			id: adminrole.id,
			allow: [PermissionFlagsBits.ViewChannel]
		},
		{
			id: racerole[schedule].id,
			allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ReadMessageHistory],
			deny: [PermissionFlagsBits.SendMessages]
		},
		{
			id: lazykidid,
			allow: [PermissionFlagsBits.ViewChannel, PermissionFlagsBits.ManageRoles]
		},
		{
			id: guild.roles.everyone,
			deny: [PermissionFlagsBits.ViewChannel]
		}
	]);
}

//*****RACE FUNCTIONS END



//*****MISC FUNCTIONS START

function CreateBotCommandsEmbed() {
	const embedd = new EmbedBuilder()
		.setTitle("Race Controls")
		.setColor("#00FFFF")
		.setDescription("Lazy Kid Controls");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('racestatus')
				.setLabel('Race Status')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('removeracer')
				.setLabel('Remove Racer')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('dqracer')
				.setLabel('DQ Racer')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('beginappeal')
				.setLabel('Open Appeal')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('closeappeal')
				.setLabel('Close Appeal')
				.setStyle(ButtonStyle.Primary)
			)

	guild.channels.cache.find(channel => channel.id === botcommandsid).send({embeds: [embedd], components: [row] });
	
	const embedd2 = new EmbedBuilder()
		.setTitle("Bot Speech")
		.setColor("#00FFFF")
		.setDescription("Talk as Lazy Kid");

	const row2 = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('sendmessage')
				.setLabel('Send Message')
				.setStyle(ButtonStyle.Primary)
			)

	guild.channels.cache.find(channel => channel.id === botcommandsid).send({embeds: [embedd2], components: [row2] });
}

function ConvertToStartTime(utcticks) {
	return utcticks.toString();
}

function ConvertToSignupsClose(utcticks) {
	var numticks = parseInt(utcticks);
	return (numticks - 120).toString();
}

function GetSeedHash(h) {
	var hash = '';
	
	var hashsplit = h.split(',');
	
	for (var i = 0; i < hashsplit.length; i++) {
		switch (hashsplit[i]) {
			case '0':
				hash = hash + '<:bow:683449650199789701> ';
				break;
			case '1':
				hash = hash + '<:boomerang:683449650300452966> ';
				break;
			case '2':
				hash = hash + '<:hookshot:683449650497454200> ';
				break;
			case '3':
				hash = hash + '<:bomb:683449651030130719> ';
				break;
			case '4':
				hash = hash + '<:mushroom:683449650812027019> ';
				break;
			case '5':
				hash = hash + '<:powder:683449650413568034> ';
				break;
			case '6':
				hash = hash + '<:icerod:683449650799181862> ';
				break;
			case '7':
				hash = hash + '<:pendant:683449650853969953> ';
				break;
			case '8':
				hash = hash + '<:bombos:683449650837192740> ';
				break;
			case '9':
				hash = hash + '<:ether:683449650719883274> ';
				break;
			case '10':
				hash = hash + '<:quake:683449650807963653> ';
				break;
			case '11':
				hash = hash + '<:lantern:683449650883461136> ';
				break;
			case '12':
				hash = hash + '<:hammer:683449651382452233> ';
				break;
			case '13':
				hash = hash + '<:shovel:683449650824740911> ';
				break;
			case '14':
				hash = hash + '<:flute:683449650812026944> ';
				break;
			case '15':
				hash = hash + '<:bugnet:683449650782404698> ';
				break;
			case '16':
				hash = hash + '<:book:683449650535333958> ';
				break;
			case '17':
				hash = hash + '<:bottle:683449650782404608> ';
				break;
			case '18':
				hash = hash + '<:greenpotion:695404216658690103> ';
				break;
			case '19':
				hash = hash + '<:somaria:683449650422087683> ';
				break;
			case '20':
				hash = hash + '<:cape:683449650656837632> ';
				break;
			case '21':
				hash = hash + '<:mirror:683449653127282695> ';
				break;
			case '22':
				hash = hash + '<:boots:683449650765627393> ';
				break;
			case '23':
				hash = hash + '<:glove:683449650455249010> ';
				break;
			case '24':
				hash = hash + '<:flippers:683449650564431885> ';
				break;
			case '25':
				hash = hash + '<:moonpearl:683449650707300372> ';
				break;
			case '26':
				hash = hash + '<:shield3:683449650799575118> ';
				break;
			case '27':
				hash = hash + '<:tunic:695404216633655337> ';
				break;
			case '28':
				hash = hash + '<:heart:695404216574804041> ';
				break;
			case '29':
				hash = hash + '<:map:695404216709152919> ';
				break;
			case '30':
				hash = hash + '<:compass:695404216767873094> ';
				break;
			case '31':
				hash = hash + '<:bigkey:695404216675729469> ';
				break;
		}
	}
	
	if (hash == '') {
		hash = 'Hash unavailable';
	}	
	
	return hash;
}

function PostImage(raceprerace) {
	try
	{
		var linesArray = fs.readFileSync('C:\\Bots\\LazyKid\\images.txt', 'utf8').split('\r\n')
		var img = '';
		var rand = Math.floor(Math.random() * Math.floor(linesArray.length));
		img = linesArray[rand];
		
		raceprerace.send(img);
	}
	catch(err)
	{
		console.log(err);
	}
}

function GetRacingRole(mode) {
	var announcementrole = racingannouncementsid;
	
	switch (mode) {
		case 'Open':
		case 'Open Boots':
		case 'Open Plus':
		case 'Dark Open':
		case 'Tournament Hard':
			announcementrole = openannouncementid;
			break;
		case 'Casual Boots':
		case 'SGL Casual Boots':
			announcementrole = casualbootsannouncementid;
			break;
		case 'Crosskeys':
		case 'Inverted Crosskeys':
		case 'Retrance':
		case 'Retrance (INV)':
		case 'Crosshunt':
			announcementrole = crosskeyannouncementid;
			break;
		case 'AA Mystery':
		case 'VT Mystery':
		case 'Champions Mystery':
			announcementrole = mysteryannouncementid;
			break;
		case 'Grab Bag':
		case 'Giga Bag':
			announcementrole = grabbagannouncementid;
			break;
		case 'Ambrosia':
			announcementrole = ambrosiaannouncementid;
			break;
		case 'AmbroZ1a':
			announcementrole = ambroz1aannouncementid;
			break;
		case 'Champ Swordless':
			announcementrole = swordlessannouncementid;
			break;
		case 'Enemizer':
			announcementrole = enemizerannouncementid;
			break;
		case 'Hundo Keysanity':
			announcementrole = hundokeysanityannouncementid;
			break;
		case 'Insanity Entrance':
			announcementrole = insanityentranceannouncementid;
			break;
		case 'Inverted Keysanity':
		case 'Inverted AD Keys':
		case 'InfluKeys':
			announcementrole = invertedkeysanityannouncementid;
			break;
		case 'Invrosia':
			announcementrole = invrosiaannouncementid;
			break;
		case 'Ludicrous Speed':
			announcementrole = ludicrousspeedannouncementid;
			break;
		case 'MC Shuffle':
		case 'McBoss':
			announcementrole = mcshuffleannouncementid;
			break;
		case 'AD Keysanity':
		case 'Open Keysanity':
		case 'FAD Keysanity':
			announcementrole = adkeysanityannouncementid;
			break;
		case 'Patron Party':
			announcementrole = patronpartyannouncementid;
			break;
		case 'Potpourri':
			announcementrole = s7potpourriannouncementid;
			break;
		case 'Reduced Crystals':
			announcementrole = reducedcrystalsannouncementid;
			break;
		case 'Spoiler Open':
			announcementrole = spoileropenannouncementid;
			break;
		case 'Standard':
		case 'Standard Boots':
		case 'Hard Standard':
		case 'Speedy Standard':
		case 'Speedy Standard (INV)':
			announcementrole = standardannouncementid;
			break;
		case 'Gold Rush':
		case 'Champions Hunt':
			announcementrole = triforcehuntannouncementid;
			break;
		case 'Double Down':
			announcementrole = doubledownannouncementid;
			break;
		case 'Pots and Bones':
		case 'True Pot Hunt':
			announcementrole = potteryannouncementid;
			break;
		case 'Beginner Doors':
		case 'Intermediate Doors':
		case 'CrissCross':
			announcementrole = doorsannouncementid;
			break;
		case 'xDHunt':
			announcementrole = keydropannouncementid;
			break;
	}
	
	return announcementrole;
}

function GetModeDef(mode) {
	var currentdef = "";
	
	switch (mode) {
		case 'AD Keysanity':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928104984338395170";
			break;
		case 'Ambrosia':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105000230604941";
			break;
		case 'AmbroZ1a':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/967538064534691911";
			break;
		case 'Beginner Doors':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1295861277587017800";
			break;
		case 'Casual Boots':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928104940767948840";
			break;
		case 'Champ Swordless':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105019847368734";
			break;
		case 'Champions Hunt':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1018538203365507112";
			break;
		case 'Champions Mystery':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1235604688301461634";
			break;
		case 'Crosshunt':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1295861309790883945";
			break;
		case 'Crosskeys':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928104956676935770";
			break;
		case 'Dark Open':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1188654167489781930";
			break;
		case 'Double Down':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1235604711512604772";
			break;
		case 'Enemizer':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105044430188594";
			break;
		case 'FAD Keysanity':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1099676524191432805";
			break;
		case 'Giga Bag':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105083126825051";
			break;
		case 'Gold Rush':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105060234330142";
			break;
		case 'Grab Bag':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105083126825051";
			break;
		case 'Hard Standard':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105101074247690";
			break;
		case 'Hundo Keysanity':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1113608473180123147";
			break;
		case 'Inverted AD Keys':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105114626060319";
			break;
		case 'Insanity Entrance':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1041415256355901571";
			break;
		case 'Insanity Entrance':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1041415256355901571";
			break;
		case 'Intermediate Doors':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1315033021388161084";
			break;
		case 'Inverted Crosskeys':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/967538152518590554";
			break;
		case 'Inverted Keysanity':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105129520017439";
			break;
		case 'Invrosia':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105144187502612";
			break;
		case 'Ludicrous Speed':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105160784367686";
			break;
		case 'MC Shuffle':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105184532508703";
			break;
		case 'McBoss':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1099676328480997417";
			break;
		case 'VT Mystery':
			currentdef = "https://discord.com/channels/680412815697117215/691998554008584202/1293009853165604934";
			break;
		case 'AA Mystery':
			currentdef = "https://discord.com/channels/680412815697117215/691998554008584202/1295862049578029066";
			break;
		case 'Open':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928104920723369994";
			break;
		case 'Open Boots':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105203545276456";
			break;
		case 'Open Keysanity':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105216220479528";
			break;
		case 'Open Plus':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/939646376831238164";
			break;
		case 'Patron Party':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1113608515710361620";
			break;
		case 'Potpourri':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105229868732448";
			break;
		case 'Pots and Bones':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1295861440745439333";
			break;
		case 'Reduced Crystals':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105245614153758";
			break;
		case 'Retrance':
		case 'Retrance (INV)':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105350081675345";
			break;
		case 'SGL Casual Boots':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105391559163974";
			break;
		case 'Spoiler Open':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105442259927130";
			break;
		case 'Standard':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/928105484177768528";
			break;
		case 'Standard Boots':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1099675762681978921";
			break;
		case 'Tournament Hard':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1099676157298872470";
			break;
		case 'True Pot Hunt':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1315033027809644618";
			break;
		case 'xDHunt':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1296169863571705946";
			break;
		case 'InfluKeys':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1342338069675901011";
			break;
		case 'CrissCross':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1342338088944406568";
			break;
		case 'Speedy Standard':
		case 'Speedy Standard (INV)':
			currentdef = "https://discord.com/channels/680412815697117215/826163116843859968/1342338090056024125";
			break;
	}
	
	return currentdef;
}

function PostFaqMessages() {
	client.channels.cache.get(faqid).send('Can\'t see anything other than this message? Go into your Discord settings, go to Text & Images, and make sure *Show embeds and preview website links pasted into chat* is checked!\n https://cdn.discordapp.com/attachments/680555967519129600/1171531744781025371/image.png');
	
	SendFaqMessage('What is the ALTTPR Ladder?', 'ALTTPR Ladder is an updated and modified "A Link To The Past Randomizer" race community, based roughly on the original ALTTPR Ladder. It consists of multiple races per day, rotating on a 24/7 schedule. We use a custom ranking system to determine overall competitive ladders for each mode. For more information, keep reading here, and we also recommend you check out the website at https://alttprladder.com or read the summary document here: https://docs.google.com/document/d/1ElnLgedHPbDjXZ3sjyt6hFMG_cPiGUGZx3DEduc9hEM\n\nAll of the information should be available for you there!');
	
	SendFaqMessage('What rules does the ALTTPR Ladder follow?', 'Please view the above document for a full set of rules and processes for ALTTPR Ladder. If you want to see which glitches are legal for ALTTPR Ladder racing, please check it out here: http://alttp.mymm1.com/wiki/Rulesets/ALTTPR_Ladder/Racing');
	
	SendFaqMessage('That\'s great! How do I join?', 'The first step is already done, you\'re here! If you haven\'t done so already, please register in by clicking on the Register button in ' + client.channels.cache.get(ladderregistrationid).toString() + '. Once you are registered, you will gain access to the race-signup channel.\n\n' + client.channels.cache.get(racesignupid).toString() + ' will announce when a race is open to be signup for. When race signups are open, you can join by clicking on "Join Race" and then following the instructions that Lazy Kid gives you when signups close in the #XXXXX-race channel, where XXXXX is the unique race name. Or feel free to ask an admin or a question in one of the channels.');
	
	SendFaqMessage('Do I need to stream my race, and do I need any delay?', 'Stream your race, yes. Delay, no. And on top of that, be sure you are saving your VODs! See below for appeals to understand why. You can stream on any platform, just as long as we can view your race immediately after your result is recorded. If you do not stream your race, you are subject to a disqualification and forfeiting your race.');

	SendFaqMessage('Can I be on a voice call during my race?', 'No, voice calls are banned during all ALTTPR races (with one exception, see below), regardless of who is on the voice call. Emergency or personal calls are okay to take during a race, as we understand those are more important than racing. Breaking this rule will result in a suspension and/or ban. The one exception to this rule is during any spoiler races, where the spoiler log is available. You are allowed to be on voice during these races as long as you do not get or give any hints or tips. If you do, it is considered cheating, and you are subject to penalties, suspensions, and/or bans.');

	SendFaqMessage('What are appeals?', 'If you feel like your race was completed in error, or you feel like an opponent\'s result isn\'t accurate, you may appeal your race. Please click on "Open Appeal" in ' + client.channels.cache.get(appealsid).toString() + ' and fill out the details requested. This will alert the Appeal Team to have your match looked into. Please read ALL of the instructions about appeals before entering one! Please do not be afraid to put in an appeal, but please be sure you are doing it for the right reasons. You will have six hours from the completion of your race to appeal, if your result needs to be changed. As long as your appeal comes in before that time, the rankings will be recalculated if a change in the result is determined. This is why VODs are so important, they are your proof! If you do not have a VOD and someone appeals the match, you have no proof that you ran the race, and thus, you will likely lose the appeal. If you put in an appeal for any other racer, there is no limit on when you can submit the appeal. You can also reach out to an admin directly.');
	
	SendFaqMessage('How do the rankings work?', 'The ALTTPR Ladder uses a custom ranking/rating system. Please see the summary document in the first post and read the section for Ratings.\n\nEach mode will have its own separate ranking and they do not effect the other ones. New rankings are calculated at the end of each race and can be viewed here: https://alttprladder.com/rankings');
	
	SendFaqMessage('Who is this Lazy Kid?', 'The Lazy Kid is your race administrator bot! You may know him as the lazy bum that sleeps in Kakariko Village, across the street from the tavern, drinking all of that Kakariko milk and then sleeping until the mid-afternoon. He\'s not sick! He\'s just lazy. But all that drinking is expensive, so he had to get a job.\n\nLazy Kid controls just about everything. He starts the races, he controls the roles, and he communicates your results back to the website. If he has something to tell you, you should probably listen.');
	
	SendFaqMessage('What if Lazy Kid screws up?', 'He\'s not perfect (that milk...) and he can occasionally make mistakes. If there is a technicial issue, be it a Discord failure, an issue with the bot, or anything that would result in results not being recorded properly, the Admins will try to manually repair them. If it cannot be done, however, the race may be voided out. We don\'t expect this to happen, but sometimes, technology just fails, and we apologize in advance if any race results need to be voided.');
	
	SendFaqMessage('I suspect So-And-So is cheating, or is an alt account.', 'Tell the Admin team! We are watching racers as closely as we can, but the more eyes, the better. Please use the appeal form in ' + client.channels.cache.get(appealsid).toString() + '. Cheating will be dealt with swiftly with bannings. Alts are not allowed in the ALTTPR Ladder and will also be promptly banned.');
	
	SendFaqMessage('Is Auto-Tracking allowed?', 'Yes! Be aware that we do not allow tracking of entrance locations or any advanced tracking. If you are caught auto-tracking anything that is not allowed, you will likely be disqualified from your race and potentially face other restrictions or bannings.');

	SendFaqMessage('Do I need to have my stream public, and where can I update it?', 'Yes, you do need to have your stream public so that your opponents can review your race if they want to. You are required to have it posted before you can join a race. You can post and update your stream location in ' + client.channels.cache.get(streamsid).toString() + '.');

	SendFaqMessage('How can I update my display name on Discord and on the website?', 'Your display name is always updated whenever you join a race. However, if you wish to manually update it after you have changed it, you can use the "Update Display Name" button in ' + client.channels.cache.get(ladderregistrationid).toString() + ' and it will push through any updates you have made.');

	SendFaqMessage('I see two races going on at the same time, can I join more than one race at a time?', 'No, you can only be actively racing in one race at a time. If you are still racing in one and want to sign up for another, you need to be finished with your race first, or forfeit, and then you may sign up for the other one. Also, abusing the finish/undo finish functionality in order to join another race will subject you to a ban. Please do not do this.');
}

function RefreshFaqChannel() {
	PostFaqMessages();
}

function SendFaqMessage(question, answer) {
	const exampleEmbed = new EmbedBuilder()
		.setColor('#FF00FF')
		.setTitle(question)
		.setDescription(answer);

	client.channels.cache.get(faqid).send({ embeds: [exampleEmbed] });
}

function RegMessage() {
	const embedd = new EmbedBuilder()
		.setTitle("ALTTPR Ladder Registration")
		.setColor("#FF00FF")
		.setDescription("You can register for Ladder by clicking on Register, or unregister by clicking on Unregister.\r\n\r\nYou will be automatically unregistered if you leave the server.\r\n\r\nIf you need to just update your displayed username, click on Update Display Name");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('register')
				.setLabel('Register')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('unregister')
				.setLabel('Unregister')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('updatename')
				.setLabel('Update Display Name')
				.setStyle(ButtonStyle.Primary),
			)

	client.channels.cache.get(ladderregistrationid).send({embeds: [embedd], components: [row] });
}

function CreateASeed() {
	const embedd = new EmbedBuilder()
	.setTitle("Roll A Seed (A => I)")
	.setColor("#ffbb33")
	.setDescription("Select from the dropdown below to roll a seed from any mode in ALTTPR Ladder")

	const row = new ActionRowBuilder()
		.addComponents(
			new StringSelectMenuBuilder()
				.setCustomId('selectmode')
				.setPlaceholder('Select Mode (A => I)')
				.addOptions([
					{
						label: 'AA Mystery',
						value: '49'
					},
					{
						label: 'AD Keysanity',
						value: '10'
					},
					{
						label: 'Ambrosia',
						value: '3'
					},
					{
						label: 'AmbroZ1a',
						value: '29'
					},
					{
						label: 'Beginner Doors',
						value: '48'
					},
					{
						label: 'Casual Boots',
						value: '8'
					},
					{
						label: 'Champions Hunt',
						value: '32'
					},
					{
						label: 'Champions Mystery',
						value: '44'
					},
					{
						label: 'Champions Swordless',
						value: '20'
					},
					{
						label: 'CrissCross',
						value: '54'
					},
					{
						label: 'Crosshunt',
						value: '46'
					},
					{
						label: 'Crosskeys',
						value: '5'
					},
					{
						label: 'Dark Open',
						value: '42'
					},
					{
						label: 'Double Down',
						value: '45'
					},
					{
						label: 'Enemizer',
						value: '4'
					},
					{
						label: 'FAD Keysanity',
						value: '37'
					},
					{
						label: 'Gold Rush',
						value: '24'
					},
					{
						label: 'Hard Standard',
						value: '15'
					},
					{
						label: 'Hundo Keysanity',
						value: '40'
					},
					{
						label: 'Insanity Entrance',
						value: '33'
					},
					{
						label: 'InfluKeys',
						value: '53'
					},					
					{
						label: 'Intermediate Doors',
						value: '52'
					}
				]),
		);

	 client.channels.cache.get(createaseed).send({embeds: [embedd], components: [row] });

	const embedd2 = new EmbedBuilder()
	.setTitle("Roll A Seed (I => Z)")
	.setColor("#ffbb33")
	.setDescription("Select from the dropdown below to roll a seed from any mode in ALTTPR Ladder")

	const row2 = new ActionRowBuilder()
		.addComponents(
			new StringSelectMenuBuilder()
				.setCustomId('selectmode')
				.setPlaceholder('Select Mode (I => Z)')
				.addOptions([				
					{
						label: 'Inverted AD Keys',
						value: '22'
					},
					{
						label: 'Inverted Crosskeys',
						value: '30'
					},
					{
						label: 'Inverted Keysanity',
						value: '7'
					},
					{
						label: 'Invrosia',
						value: '11'
					},
					{
						label: 'Ludicrous Speed',
						value: '26'
					},
					{
						label: 'MC Shuffle',
						value: '16'
					},
					{
						label: 'MCBoss',
						value: '35'
					},
					{
						label: 'Open',
						value: '1'
					},
					{
						label: 'Open Boots',
						value: '12'
					},
					{
						label: 'Open Keysanity',
						value: '9'
					},
					{
						label: 'Open Plus',
						value: '28'
					},
					{
						label: 'Patron Party',
						value: '39'
					},
					{
						label: 'Potpourri',
						value: '18'
					},
					{
						label: 'Pots and Bones',
						value: '47'
					},
					{
						label: 'Reduced Crystals',
						value: '13'
					},
					{
						label: 'Retrance',
						value: '17'
					},
					{
						label: 'SGL Casual Boots',
						value: '21'
					},
					{
						label: 'Speedy Standard',
						value: '55'
					},
					{
						label: 'Spoiler Open',
						value: '14'
					},
					{
						label: 'Standard',
						value: '2'
					},
					{
						label: 'Standard Boots',
						value: '36'
					},
					{
						label: 'Tournament Open',
						value: '38'
					},
					{
						label: 'True Pot Hunt',
						value: '51'
					},
					{
						label: 'VT Mystery',
						value: '6'
					},
					{
						label: 'xDHunt',
						value: '50'
					}
				]),
		);

	 client.channels.cache.get(createaseed).send({embeds: [embedd2], components: [row2] });
}

function AppealMessage() {
	const embedd = new EmbedBuilder()
		.setTitle("Official Appeal Request")
		.setColor("#FF0000")
		.setDescription("If you want to request an appeal of your race, please be sure you are within 24 hours of the completion of your race, and you have applicable VODs available for review.\n\nProper reasons for an appeal:\n\nMistake in Finish Time\nIf there was a mistake in your finish time, and you are unable to undo your finish yourself, you can either inform an admin or put in an appeal. We will update your times manually.\n\nVOD Review\nIf you want the admin team to investigate any racer’s VOD, after you have first reviewed it. We will review this on a case by case basis, but please have a valid reason for doing so and not doing it just to have us review something random.\n\nROM Crashing\nIf your ROM crashes due to a technical issue in the ROM itself, you may submit an appeal. However, unless it is an issue that affects all racers, the appeal will likely be denied. We will judge these on a case-by-case basis.\n\nImproper reasons for an appeal:\n\nDo not put in an appeal if you think your seed is broken. It isn't. Broken seeds are not something that occur, outside of fringe cases in mystery seeds. Wait for your opponents to finish, and then review it with them. You'll likely find out what you missed or did wrong. In the rare chance of an actual broken seed, we will nullify the race.\n\nDo not put in an appeal for a technical issue, like your emulator crashing, or a power outage. It sucks, but we cannot reverse results because of any technical issue on your end.\n\nPlease click on Open Appeal, fill out the details on the form, and then click Submit. The Appeals Team will be alerted to contact you and investigate.\n\nDO NOT ABUSE THIS FUNCTION! WE WILL BAN ANYBODY WHO ABUSES THIS SYSTEM!\n\nPlease review the appeal procedure above before putting in your appeal.");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('openappeal')
				.setLabel('Open Appeal')
				.setStyle(ButtonStyle.Danger),
			);

	 client.channels.cache.get(appealsid).send({embeds: [embedd], components: [row] });
}

function StreamsMessage() {
	const embedd = new EmbedBuilder()
		.setTitle("Set or Search Stream")
		.setColor("#FFFF00")
		.setDescription("To set your current stream, click Set Stream. Be sure this is accurate if you have changed anything with your active stream, as that is where we will look for VODs.\n\nTo search for a stream, click Search Stream. Be sure to use the racer\'s username, which you can get by right clicking on their user, selecting Profile, and it is the smaller text under the display name. If they have a discriminator tag (#XXXX), please be sure to include that with your search.");

	const row = new ActionRowBuilder()
		.addComponents(
			new ButtonBuilder()
				.setCustomId('setstream')
				.setLabel('Set Stream')
				.setStyle(ButtonStyle.Primary),
			new ButtonBuilder()
				.setCustomId('searchstream')
				.setLabel('Search Stream')
				.setStyle(ButtonStyle.Primary),
			)

	client.channels.cache.get(streamsid).send({embeds: [embedd], components: [row] });
}

function GetDisplayNickname(member) {
	var n = member.nickname;
	
	if (n == null) {
		n = member.user.globalName;
	}
	
	if (n == null) {
		n = member.user.username;
	}
	
	return n;
}

function GetRacerLogin(member) {
	var n = member.user.username;
	
	if (member.user.discriminator != null && member.user.discriminator != "0") {
		n = n + "--" + member.user.discriminator;
	}
	
	return n;
}

async function ClearChannel(channel, n = 0, old = false) {
	try
	{
	  let collected = await channel.messages.fetch({ limit: 100 });
	  if (collected.size > 0) {
		if (old) {
		  for (let msg of collected.array()) {
			await msg.delete().catch(err => err);
			n++;
		  }
		} else {
		  let deleted = await channel.bulkDelete(100, true);
		  if (deleted.size < collected.size) old = true;
		  n += deleted;
		}

		return n + await ClearChannel(channel, old);
	  } else return 0;		
	}
	catch
	{
	}
	
	return 0;
}

function logMessage(channel, user, message) {
	var logfile = basedir + channel + '.txt';
	
	fs.appendFile(logfile, new Date().toLocaleString() + "\t" + user + "\t" + message + '\r\n', function (err) {
		if (err) throw err;
	});
};

//*****MISC FUNCTIONS END

