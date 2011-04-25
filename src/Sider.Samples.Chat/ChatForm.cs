
using System;
using System.Concurrency;
using System.Linq;

namespace Sider.Samples.Chat
{
  public partial class ChatForm : RedisFormBase
  {
    public ChatForm(string name, string room)
    {
      InitializeComponent();

      Text = "Chat: " + room;

      // keys for chatting
      var joins = room + ":joins";
      var leaves = room + ":leaves";
      var members = room + ":mems";
      var messages = room + ":msg";

      // needs 2 client, one for subscribing and one for publishing
      var msgClient = GetClient();
      SayButton.Click += (sender, e) =>
        msgClient.Publish(messages, name + " > " + ChatBox.Text);

      var client = GetClient();
      client.Publish(joins, name);
      client.SAdd(members, name);

      ParticipantList.Items.AddRange(client.SMembers(members));

      var obs = client.PSubscribe(room + "*")
        .ObserveOn(Scheduler.Dispatcher)
        .Subscribe(m =>
        {
          if (m.SourceChannel == joins)
            ParticipantList.Items.Add(m.Body);
          else if (m.SourceChannel == leaves)
            ParticipantList.Items.Remove(m.Body);
          else if (m.SourceChannel == messages)
            MsgList.Items.Add(m.Body);
        });

      this.FormClosing += (sender, e) =>
      {
        client.PUnsubscribe(room + "*");
        obs.Dispose();

        client.Publish(leaves, name);
        client.Dispose();
      };
    }
  }
}
