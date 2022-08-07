using Discord;                //discord
using Discord.Commands;       //discord
using Discord.WebSocket;      //discord
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ScruffiDiscordBot
{
    internal static class Program
    {
        #region Values
        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;
        public static bool isLogged = false;
        public static List<string> RandomIDS = new List<string>();
        public static ulong LogChannelID = 920378014687174666;
        public static ulong MainRoleID = 920338308612100149;
        #endregion



        [STAThread]
        static void Main(string[] args)
        {

            Application.EnableVisualStyles();

            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());

        }
        public static async Task MainAsync()
        {

            Console.WriteLine("Started");
            var tststr1 = Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\"));
            var tststr2 = tststr1.Remove(tststr1.LastIndexOf("\\"));
            var configFilePath = tststr2.Remove(tststr2.LastIndexOf("\\"));
           var config = File.ReadAllText(configFilePath + "\\" + "config.txt");

            config = config.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\r", "\n");
            var splittedtext = config.Split('\n');

            MainRoleID =ulong.Parse(splittedtext[0].Substring(splittedtext[0].IndexOf("BotUserRoleID:") + "BotUserRoleID:".Length));
            LogChannelID = ulong.Parse(splittedtext[1].Substring(splittedtext[1].IndexOf("WhoClickedButtonChannelID:") + "WhoClickedButtonChannelID:".Length));
            string token =splittedtext[2].Substring(splittedtext[2].IndexOf("BotToken:") + "BotToken:".Length);
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildVoiceStates | GatewayIntents.All
                
            });
            _commands = new CommandService();

           
            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();
            _client.Log += Log;

         

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

         
            
            _client.MessageReceived += MessageRecieve;
            _client.ButtonExecuted += _client_ButtonExecuted;
          
            await Task.Delay(-1);


        }

     

        private static async Task _client_ButtonExecuted(SocketMessageComponent e)
        {
            if (e.Data.CustomId != null)
            {
                if (e.User as IGuildUser != null)
                {
                    if (e.Data.CustomId.ToLower().StartsWith("blue") ||
                        e.Data.CustomId.ToLower().StartsWith("gray") ||
                        e.Data.CustomId.ToLower().StartsWith("green") ||
                        e.Data.CustomId.ToLower().StartsWith("red"))
                    {

                        var chnl = await _client.GetChannelAsync(LogChannelID);
                        if (chnl != null)
                        {
                            var channel = chnl as ITextChannel;

                            if (channel != null)
                            {
                                await channel.SendMessageAsync("**This user:** " + e.User.Mention + " **Clicked " + e.Data.CustomId + "**");
                            }
                        }

                    }

                }
            }



        }

        static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


       

        static async Task MessageRecieve(SocketMessage message)
        {
            var gld = (message.Channel as ITextChannel).Guild;
            var usrs = await gld.GetUsersAsync();

           var chnl =await message.Channel.GetMessageAsync(21312);
           
           foreach(var m in usrs.ToList())
            {
               if(m.Activities.ToList().Count > 0)
                {
                    Console.WriteLine(m.Activities.ToList()[0].ToString());
                }
                else
                {
                    Console.WriteLine("x");
                }
            }
         
            if (message.Content.StartsWith(".linkbutton "))
            {
                if (message.Author as IGuildUser != null)
                {
                    if ((message.Author as IGuildUser).RoleIds.ToList().Contains(MainRoleID))
                    {
                        var user = message.Author as SocketGuildUser;
                        if (user != null)
                        {
                            if (message.MentionedChannels != null)
                            {

                                if (message.MentionedChannels.Count > 0)
                                {

                                    string msgString = "";
                                    List<string> linksinmessage = new List<string>();
                                    foreach (Match item in Regex.Matches(message.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                                    {
                                        linksinmessage.Add(item.Value);
                                    }

                                    if (linksinmessage.Count > 0)
                                    {


                                        var builder = new ComponentBuilder().WithButton("Open the link!", null, ButtonStyle.Link, null, linksinmessage[0], false, 4);

                                        
                                        var afterString = message.Content.Substring(message.Content.IndexOf(linksinmessage[0]) + linksinmessage[0].Length);

                                        if(afterString.Length > 1)
                                        { 
                                            try
                                            {
                                                msgString = afterString.Substring(afterString.IndexOf("&") + 1);
                                            }
                                            catch
                                            {
                                                msgString = "";
                                            }
                                            string butName = "";

                                            try
                                            {
                                                if(msgString.Length > 0)
                                                {
                                                    butName = afterString.Replace("&"+msgString, "");
                                                }
                                                else
                                                {
                                                    butName = afterString;
                                                }
                                            }
                                            catch
                                            {
                                                butName = "";
                                            }

                                            Console.WriteLine(butName);

                                            if(butName.Length > 1)
                                            {
                                                if(!butName.Contains("&"))
                                                {
                                                    builder = new ComponentBuilder().WithButton(butName, null, ButtonStyle.Link, null, linksinmessage[0], false, 4);
                                                }
                                                else
                                                {
                                                    builder = new ComponentBuilder().WithButton("Open this link!", null, ButtonStyle.Link, null, linksinmessage[0], false, 4);
                                                }
                                             
                                            }
                                            else
                                            {
                                                builder = new ComponentBuilder().WithButton("Open this link!", null, ButtonStyle.Link, null, linksinmessage[0], false, 4);
                                            }

                                            foreach (var m in message.MentionedChannels)
                                            {
                                                if (m as ITextChannel != null)
                                                {

                                                    var channel = m as ITextChannel;
                                                    if (msgString.Length > 0)
                                                    {
                                                        await channel.SendMessageAsync(msgString, false, null, null, null, null, builder.Build());
                                                    }
                                                    else
                                                    {
                                                        await channel.SendMessageAsync("‏‏‏‏‏‏‏‏   ", false, null, null, null, null,  builder.Build());
                                                    }

                                                }

                                            }

                                        }
                                        else
                                        {
                                            foreach (var m in message.MentionedChannels)
                                            {
                                                if (m as ITextChannel != null)
                                                {

                                                    var channel = m as ITextChannel;
                                                    if(msgString.Length > 0)
                                                    {
                                                        await channel.SendMessageAsync(msgString, false, null, null, null, null,   builder.Build());
                                                    }
                                                    else
                                                    {
                                                        await channel.SendMessageAsync("‏‏‏‏‏‏‏‏   ", false, null, null, null, null, builder.Build());
                                                    }
                                                   
                                                }

                                            }
                                        }
                                     



                                    }
                                }

                            }

                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("You've no permission to do this!", false, null, null, null, message.Reference);
                    }
                }
             

            }

            if (message.Content.StartsWith(".sendfile "))
            {
                if (message.Author as IGuildUser != null)
                {
                    if ((message.Author as IGuildUser).RoleIds.ToList().Contains(MainRoleID))
                    {
                        var user = message.Author as SocketGuildUser;
                        if (user != null)
                        {
                            if (message.MentionedChannels != null)
                            {
                                if (message.MentionedChannels.Count > 0)
                                {
                                    if (message.Attachments != null)
                                    {
                                        if (message.Attachments.Count > 0)
                                        {






                                            foreach (var tempchannel in message.MentionedChannels)
                                            {
                                                var channel = tempchannel as ITextChannel;
                                                if (channel != null)
                                                {
                                                    var msgforyou = message.Content.Substring(message.Content.LastIndexOf(message.MentionedChannels.Last().Id.ToString() + ">") +(message.MentionedChannels.Last().Id.ToString() + ">").Length);
                                                    if (msgforyou.Length > 1)
                                                    {
                                                        await channel.SendMessageAsync(msgforyou);

                                                        foreach (var file in message.Attachments)
                                                        {
                                                            await channel.SendMessageAsync(file.Url);
                                                        }

                                                    }
                                                    else
                                                    {
                                                        await channel.SendMessageAsync("‏‏‏‏‏‏‏‏   ");

                                                        foreach (var file in message.Attachments)
                                                        {
                                                            await channel.SendMessageAsync(file.Url);
                                                        }
                                                    }


                                                }

                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("You've no permission to do this!", false, null, null, null, message.Reference);
                    }
                }
              

            }

            if (message.Content.StartsWith(".button "))
            {
                if (message.Author as IGuildUser != null)
                {
                    if ((message.Author as IGuildUser).RoleIds.ToList().Contains(MainRoleID))
                    {
                        if (message.MentionedChannels.Count > 0)
                        {
                            var basestring = "";
                            try
                            {
                                basestring = message.Content.Substring(message.Content.IndexOf(message.MentionedChannels.Last().Id + ">") + (message.MentionedChannels.Last().Id + ">").Length + 1);
                            }
                            catch
                            {
                                basestring = null;
                            }



                            string buttontype = "";
                            if (basestring != null)
                            {
                                try
                                {
                                    buttontype = basestring.Remove(basestring.IndexOf(" ")).ToLower();
                                }
                                catch
                                {
                                    buttontype = null;

                                }

                                if (string.IsNullOrWhiteSpace(buttontype))
                                {
                                    buttontype = null;
                                }
                            }
                            else
                            {
                                buttontype = null;
                            }





                            string msg = "";

                            if (basestring != null)
                            {
                                if (buttontype != null)
                                {
                                    try
                                    {
                                        msg = basestring.Substring(basestring.IndexOf(buttontype) + buttontype.Length + 1);
                                    }
                                    catch
                                    {
                                        msg = null;
                                    }
                                }
                                else
                                {
                                    msg = null;
                                }

                            }
                            else
                            {
                                msg = null;
                            }

                            





                            var button = new ComponentBuilder();

                            if (buttontype != null)
                            {

                                if (msg != null)
                                {
                                    if (buttontype == "gray" || buttontype == "grey")
                                    {
                                        button.WithButton(msg, "Gray Button created at: " + message.Timestamp.ToString()+"\n Button named: " + msg, ButtonStyle.Secondary);

                                    }
                                    else if (buttontype == "green")
                                    {
                                        button.WithButton(msg, "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + msg, ButtonStyle.Success);

                                    }
                                    else if (buttontype == "blue")
                                    {
                                        button.WithButton(msg, "Blue Button created at: " + message.Timestamp.ToString()+"\n Button named: " + msg, ButtonStyle.Primary);

                                    }
                                    else if (buttontype == "red")
                                    {
                                        button.WithButton(msg, "Red Button created at: " + message.Timestamp.ToString()+"\n Button named: " + msg, ButtonStyle.Danger);

                                    }
                                    else
                                    {
                                        button.WithButton(basestring, "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + basestring, ButtonStyle.Success);
                                    }




                                }
                                else
                                {
                                    if (buttontype == "gray" || buttontype == "grey")
                                    {
                                        button.WithButton("Click this button!", "Gray Button created at: " + message.Timestamp.ToString() +"\n Button named: " + "Click this button!", ButtonStyle.Secondary);

                                    }
                                    else if (buttontype == "green")
                                    {
                                        button.WithButton("Click this button!", "Green Button created at: " + message.Timestamp.ToString() +"\n Button named: " + "Click this button!", ButtonStyle.Success);

                                    }
                                    else if (buttontype == "blue")
                                    {
                                        button.WithButton("Click this button!", "Blue Button created at: " + message.Timestamp.ToString() + "\n *Button named: " + "Click this button!", ButtonStyle.Primary);

                                    }
                                    else if (buttontype == "red")
                                    {
                                        button.WithButton("Click this button!", "Red Button created at: " + message.Timestamp.ToString() +"\n Button named: " + "Click this button!", ButtonStyle.Danger);

                                    }
                                    else
                                    {
                                        button.WithButton("Click this button!", "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Success);

                                    }
                                }
                            }
                            else
                            {
                                if (basestring != null)
                                {

                                    if (basestring.Length > 1)
                                    {

                                        if (basestring == "gray" || basestring == "grey")
                                        {
                                            button.WithButton("Click this button!", "Gray Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Secondary);

                                        }
                                        else if (basestring == "green")
                                        {
                                            button.WithButton("Click this button!", "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Success);

                                        }
                                        else if (basestring == "blue")
                                        {
                                            button.WithButton("Click this button!", "Blue Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Primary);

                                        }
                                        else if (basestring == "red")
                                        {
                                            button.WithButton("Click this button!", "Red Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Danger);

                                        }
                                        else
                                        {

                                            button.WithButton(basestring, "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + basestring +"**", ButtonStyle.Success);

                                        }

                                    }
                                    else
                                    {
                                        button.WithButton("Click this button!", "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Success);
                                    }
                                }
                                else
                                {
                                    button.WithButton("Click this button!", "Green Button created at: " + message.Timestamp.ToString()+"\n Button named: " + "Click this button!", ButtonStyle.Success);
                                }

                            }

                            foreach (var channel in message.MentionedChannels)
                            {
                                var chnl = channel as ITextChannel;

                                if (chnl != null)
                                {
                                    
                                    await chnl.SendMessageAsync("‏‏‏‏‏‏‏‏ ", false, null, null, null, null, button.Build());
                                }
                            }
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("You've no permission to do this!", false, null, null, null, message.Reference);
                    }
                }

               

            }

            if (message.Content.StartsWith(".role "))
            {
                if(message.MentionedRoles.Count > 0)
                {
                    if(message.MentionedUsers.Count > 0)
                    {
                        List<SocketRole> roles = new List<SocketRole>();
                        roles.AddRange(message.MentionedRoles);
                        foreach(var usr in message.MentionedUsers)
                        {
                            var user = usr as SocketGuildUser;
                         

                            if ((message.Channel as IGuildChannel).Guild != null)
                            {
                   
                                var channel = message.Channel as ITextChannel;
                            
                                if(user != null)
                                {
                              
                                    user = await channel.GetUserAsync(user.Id) as SocketGuildUser;
                                    if(channel != null)
                                    {
                                     
                                        var bot = await channel.Guild.GetUserAsync(_client.CurrentUser.Id);
                                        if(bot != null)
                                        {
                                          
                                            if (user != null)
                                            {
                                              
                                                var allroles = channel.Guild.Roles;

                                                for (int i = 0; i < roles.Count; i++)
                                                {
                                                  
                                                    if (roles[i] != null)
                                                    {
                                                      

                                                        bool isEnough = false;

                                                        foreach (var m in (bot as SocketGuildUser).Roles)
                                                        {
                                                      
                                                            if (m.CompareTo(roles[i]) > 0)
                                                            {
                                                                isEnough = true;
                                                            }

                                                        }

                                                        if (isEnough)
                                                        {
                                                            if (user.Roles.Contains(roles[i]))
                                                            {

                                                                
                                                                   
                                                                        await user.RemoveRoleAsync(roles[i] as IRole);
                                                                        await message.Channel.SendMessageAsync(message.Author.Mention + "** Updated " + user.Mention + " Roles succesfully. :white_check_mark: \n Removed **" + roles[i].Mention);
                                                                   

                                                              

                                                            }
                                                            else
                                                            {
                                                                try
                                                                {
                                                                    if (user.AddRoleAsync(roles[i] as IRole) != null)
                                                                    {
                                                                        await user.AddRoleAsync(roles[i]);
                                                                        await message.Channel.SendMessageAsync(message.Author.Mention + "** Updated " + user.Mention + " Roles succesfully. :white_check_mark: \n Added **" + roles[i].Mention);
                                                                    }
                                                                    else
                                                                    {
                                                                       
                                                                    }


                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Console.WriteLine(ex.Message);
                                                                }

                                                            }


                                                        }
                                                        else
                                                        {
                                                            if (!user.Roles.Contains(roles[i]))
                                                            {
                                                                await message.Channel.SendMessageAsync("**I've no permission to** "+ "**add** " + roles[i].Mention + " ** to** " +  user.Mention + " :no_entry_sign:");
                                                            }
                                                            else
                                                            {
                                                                await message.Channel.SendMessageAsync("**I've no permission to** " + "** remove** " +roles[i].Mention +" **from** "+user.Mention + " :no_entry_sign:");
                                                            }

                                                        }
                                                        await Task.Delay(1000);

                                                    }

                                                }
                                            }
                                        }
                                    }

                                    
                                   
                                }
                              

                            
                            }
                         

                        }
                    }
                }
            }

            if (message.Content == ".help")
            {
              
                var embed = new EmbedBuilder().WithTitle("**Command prefix!**").AddField("**.linkbutton:**", "**.linkbutton #channel(s) {url} <buttonName> &<message>**")
                    .AddField("**.sendimage:**", "**.sendfile #channel(s) {attachments} <message>**")
                    .AddField("**.button:**", "**.button #channel(s) {grey/green/blue/red} <message>**")
                    .AddField("**.role:**", "**.role @user(s) @role(s)**")
                    .WithColor(Color.Purple).WithFooter(new EmbedFooterBuilder().WithText("Developed by: LindaMosep").WithIconUrl("https://c.tenor.com/s-eCb5CxKV4AAAAM/dog-deal-with-it.gif"))
                    .WithThumbnailUrl("https://64.media.tumblr.com/a7b77c9a9cd606d3d76bf626f7fe3e4a/b2ed56ab7ff3e3da-f0/s500x750/86830275e58f79938ac6f973811837dd95a961a0.gif");
                await message.Author.SendMessageAsync(" ‏‏‏‏‏‏‏‏", false, embed.Build());
            }

           
        }


    }


}
