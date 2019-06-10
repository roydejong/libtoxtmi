using System;
using System.Collections.Generic;
using System.Linq;

namespace libtoxtmi.Models
{
    /// <summary>
    /// TMI message data, with chat message-specific data.
    /// </summary>
    public class TmiChatMessage : TmiMessage
    {
        #region Consts
        /// <summary>
        /// Property key for the badge meta information list attached to this message.
        /// </summary>
        public const string PROP_BADGE_INFO = "badge-info";
        /// <summary>
        /// Property key for the badge list attached to this message.
        /// </summary>
        public const string PROP_BADGES = "badges";
        /// <summary>
        /// Property key for the sender username color (hex code).
        /// </summary>
        public const string PROP_COLOR = "color";
        /// <summary>
        /// Property key for the sender display name.
        /// </summary>
        public const string PROP_DISPLAY_NAME = "display-name";
        /// <summary>
        /// Property key for the emote sets in this message.
        /// </summary>
        public const string PROP_EMOTE_SETS = "emote-sets";
        /// <summary>
        /// Property key for the sender user id.
        /// </summary>
        public const string PROP_USER_ID = "user-id";
        /// <summary>
        /// Property key for total bits value of the message.
        /// </summary>
        public const string PROP_BITS = "bits";
        /// <summary>
        /// Property key for TMI server timestamp (when did server receive this message).
        /// </summary>
        public const string PROP_TIMESTAMP = "tmi-sent-ts";
        /// <summary>
        /// Property key for chat room id.
        /// NB: A channel may have multiple rooms.
        /// </summary>
        public const string PROP_ROOM_ID = "room-id";
        /// <summary>
        /// Property key for moderator status.
        /// Probably meant to be deprecated, as badges have replaced this kind of field.
        /// </summary>
        public const string PROP_MOD = "mod";
        /// <summary>
        /// Property key for server message id.
        /// </summary>
        public const string PROP_MESSAGE_ID = "id";

        /// <summary>
        /// Internal badge info name for subscription information (amount of months subscribed).
        /// </summary>
        public const string BADGEINFO_KEY_SUBSCRIBER = "subscriber";

        /// <summary>
        /// Internal badge name for Twitch admins (staff).
        /// </summary>
        public const string BADGE_KEY_ADMIN = "admin";
        /// <summary>
        /// Internal badge name for bit cheering.
        /// The version reflects the amount cheered over a certain period.
        /// </summary>
        public const string BADGE_KEY_BITS = "bits";
        /// <summary>
        /// Internal badge name for the channel owner / broadcaster.
        /// </summary>
        public const string BADGE_KEY_BROADCASTER = "broadcaster";
        /// <summary>
        /// Internal badge name for Twitch global moderators (staff).
        /// </summary>
        public const string BADGE_KEY_GLOBAL_MOD = "global_mod";
        /// <summary>
        /// Internal badge name for channel moderators.
        /// </summary>
        public const string BADGE_KEY_MODERATOR = "moderator";
        /// <summary>
        /// Internal badge name for channel subscribers.
        /// The version indicates what version of the subscriber badge they have, based on how long they have been subscribed.
        /// </summary>
        public const string BADGE_KEY_SUBSCRIBER = "subscriber";
        /// <summary>
        /// Interal badge name for Twitch staff (wrench).
        /// </summary>
        public const string BADGE_KEY_STAFF = "staff";
        /// <summary>
        /// Internal badge name for Twitch Turbo.
        /// </summary>
        public const string BADGE_KEY_TURBO = "turbo";
        /// <summary>
        /// Internal badge name for verified Twitch partners.
        /// </summary>
        public const string BADGE_KEY_PARTNER = "partner";
        /// <summary>
        /// Internal badge name for ??? Twitch Prime, maybe ???.
        /// </summary>
        public const string BADGE_KEY_PREMIUM = "premium";
        /// <summary>
        /// Internal badge name for bit cheering, charity gifts version.
        /// The version reflects the amount cheered/gifted over a certain period.
        /// </summary>
        public const string BADGE_KEY_BITS_CHARITY = "bits-charity";
        #endregion

        #region Raw getter props
        /// <summary>
        /// Raw property value for badge meta information.
        /// </summary>
        public string BadgeInfoRaw { get { return GetStateProp(PROP_BADGE_INFO); } }
        /// <summary>
        /// Raw property value for badge list.
        /// </summary>
        public string BadgesRaw { get { return GetStateProp(PROP_BADGES); } }
        /// <summary>
        /// Raw property value for sender username color (hex code).
        /// </summary>
        public string Color { get { return GetStateProp(PROP_COLOR); } }
        /// <summary>
        /// Raw property value for sender display name.
        /// </summary>
        public string UserDisplayName { get { return GetStateProp(PROP_DISPLAY_NAME); } }
        /// <summary>
        /// Raw property value for emote set.
        /// </summary>
        public string EmoteSetsRaw { get { return GetStateProp(EmoteSetsRaw); } }
        #endregion

        #region Prop value iterators
        /// <summary>
        /// Gets a collection of badge meta information (badge name mapped to version/value), that were attached to this chat message.
        /// </summary>
        public Dictionary<string, int> GetBadgeInfo()
        { 
            var raw = BadgeInfoRaw;        
            var list = new Dictionary<string, int>();

            if (String.IsNullOrEmpty(raw))
                return list;

            var parts = raw.Split(',');

            foreach (var part in parts)
            {
                var subParts = part.Split('/');

                var badge = subParts[0];
                var version = int.Parse(subParts[1]);

                list[badge] = version;
            }

            return list;
        }

        /// <summary>
        /// Gets a collection of badges (badge name mapped to version/value), that were attached to this chat message.
        /// </summary>
        public Dictionary<string, int> GetBadges()
        {
            var raw = BadgesRaw;
            var list = new Dictionary<string, int>();

            if (String.IsNullOrEmpty(raw))
                return list;

            var parts = raw.Split(',');

            foreach (var part in parts)
            {
                var subParts = part.Split('/');

                var badge = subParts[0];
                var version = int.Parse(subParts[1]);

                list[badge] = version;
            }

            return list;
        }

        /// <summary>
        /// Gets a plain list of badge names, without version data, attached to this chat message.
        /// </summary>
        public IEnumerable<string> GetBadgeNames()
        {
            var raw = BadgesRaw;

            if (!String.IsNullOrEmpty(raw))
            {
                var parts = raw.Split(',');

                foreach (var part in parts)
                {
                    var subParts = part.Split('/');
                    yield return subParts[0];
                }
            }
        }
        #endregion

        #region Value helpers
        /// <summary>
        /// The sender's Twitch UID.
        /// </summary>
        public long UserId
        {
            get
            {
                return long.Parse(GetStateProp(PROP_USER_ID));
            }
        }

        /// <summary>
        /// The total amount of bits cheered in this message.
        /// Returns zero if this is not a bits cheer message.
        /// </summary>
        public int BitsCheerAmount
        {
            get
            {
                var raw = GetStateProp(PROP_BITS);

                if (string.IsNullOrEmpty(raw))
                    return 0;

                return int.Parse(raw);
            }
        }

        /// <summary>
        /// Gets whether this message had the "mod" flag:
        /// if the user has a moderator badge.
        /// </summary>
        public bool IsMod
        {
            get
            {
                var raw = GetStateProp(PROP_MOD);
                if (raw != null && raw.ToString() == "1")
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets whether this message sender had an admin (staff) badge.
        /// </summary>
        public bool IsAdmin { get { return GetBadgeNames().Contains(BADGE_KEY_ADMIN); } }
        /// <summary>
        /// Gets whether this message sender had a bits donator badge.
        /// </summary>
        public bool IsBitsDonator { get { return GetBadgeNames().Contains(BADGE_KEY_BITS); } }
        /// <summary>
        /// Gets whether this message sender had a broadcaster badge.
        /// </summary>
        public bool IsBroadcaster { get { return GetBadgeNames().Contains(BADGE_KEY_BROADCASTER); } }
        /// <summary>
        /// Gets whether this message sender had a global moderator (staff) badge.
        /// </summary>
        public bool IsGlobalMod { get { return GetBadgeNames().Contains(BADGE_KEY_GLOBAL_MOD); } }
        /// <summary>
        /// Gets whether this message sender had a moderator badge.
        /// </summary>
        public bool IsModerator { get { return GetBadgeNames().Contains(BADGE_KEY_MODERATOR); } }
        /// <summary>
        /// Gets whether this message sender had a subscriber badge.
        /// </summary>
        public bool IsSubscriber { get { return GetBadgeNames().Contains(BADGE_KEY_SUBSCRIBER); } }
        /// <summary>
        /// Gets whether this message sender had a staff badge.
        /// </summary>
        public bool IsStaff { get { return GetBadgeNames().Contains(BADGE_KEY_STAFF); } }
        /// <summary>
        /// Gets whether this message sender had a turbo badge.
        /// </summary>
        public bool IsTurbo { get { return GetBadgeNames().Contains(BADGE_KEY_TURBO); } }
        /// <summary>
        /// Gets whether this message sender had a partner (verified) badge.
        /// </summary> 
        public bool IsPartner {  get { return GetBadgeNames().Contains(BADGE_KEY_PARTNER); } }
        /// <summary>
        /// Gets whether this message sender had a premium badge (turbo?).
        /// </summary>
        public bool IsPremium { get { return GetBadgeNames().Contains(BADGE_KEY_PREMIUM); } }
        /// <summary>
        /// Gets whether this message sender had a bits charity donator badge.
        /// </summary>
        public bool IsBitsCharityDonator { get { return GetBadgeNames().Contains(BADGE_KEY_BITS_CHARITY); } }

        /// <summary>
        /// Gets the subscriber badge level.
        /// This does not same as the amount of months, but the sub badge level.
        /// Level zero is the first level.
        /// </summary>
        /// <returns>The badge level. Starting at 0, or -1 if the user does not have a badge.</returns>
        public int GetSubscriberBadgeLevel()
        {
            var badges = GetBadges();

            if (badges.ContainsKey(BADGE_KEY_SUBSCRIBER))
                return badges[BADGE_KEY_SUBSCRIBER];

            return -1;
        }

        /// <summary>
        /// Gets the amount of months this user has been subscribed.
        /// This is seperate from the badge level.
        /// </summary>
        public int GetMonthsSubscribed()
        {
            var badgeInfo = GetBadgeInfo();

            if (badgeInfo.ContainsKey(BADGEINFO_KEY_SUBSCRIBER))
                return badgeInfo[BADGEINFO_KEY_SUBSCRIBER];

            return 0;
        }

        /// <summary>
        /// Gets the bit amount tied to the user's bit cheer badge.
        /// This seems to reflect the total amount of donated bits in some period.
        /// </summary>
        public int GetBitsBadgeLevel()
        {
            var badges = GetBadges();

            if (badges.ContainsKey(BADGE_KEY_BITS))
                return badges[BADGE_KEY_BITS];

            return 0;
        }
        #endregion
    }
}