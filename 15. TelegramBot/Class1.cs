using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using WTelegram;
    class User{
        private Telegram.Bot.Types.User chatId;
        public Telegram.Bot.Types.User ChatId { get; set; }
        public User(Telegram.Bot.Types.User ID){
            ChatId = ID;
        }
    }
    class Room{
        private DateTime startTime;
        public DateTime StartTime{ get; set;}
        private DateTime endTime;
        public DateTime EndTime{ get; set;}
        public Room(){ }
        public Room(DateTime startTime){
            StartTime = startTime;
            EndTime = startTime.AddMinutes(15);
            Console.WriteLine("Создал комнату");
        }
        public List<User> Roommates = new List<User>();
        public string RoommatesContacts(){
            string output = "";
            foreach(User user in Roommates){
                output = output + "\n" + user.ChatId.Username;
            }
            return output;
        }
        private int quantityOfRoommates;
        public int QuantityOfRoommates{ get{return quantityOfRoommates;} set {Roommates.Count();}}
        public async Task AddRoommate(User user, ITelegramBotClient botClient){
            if(QuantityOfRoommates < 4){
                Roommates.Add(user);
                Console.WriteLine("Добавили челика в комнату");
            }
            else{
                await botClient.SendTextMessageAsync(
                    chatId: user.ChatId.Id,
                    text: "Комната переполнена, выбери другую"
                );
                Console.WriteLine("Послали челика в другую комнату");
            }
        }
    }

