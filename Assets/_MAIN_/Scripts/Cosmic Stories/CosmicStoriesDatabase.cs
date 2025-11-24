using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MythologyStory
{
    public string type;   // e.g., "Hindu Mythology"
    public string title;  // e.g., "Surya – The Sun God"
    public string story;  // Full story text

    public MythologyStory(string type, string title, string story)
    {
        this.type = type;
        this.title = title;
        this.story = story;
    }
}

public class CosmicStoriesDatabase : MonoBehaviour
{
    public List<MythologyStory> allStories = new List<MythologyStory>();

    private void Awake()
    {
        LoadStories();
    }

    private void LoadStories()
    {
        allStories = new List<MythologyStory>()
        {
            // 1. Surya – The Sun God
            new MythologyStory(
                "Hindu Mythology",
                "Surya – The Sun God",
                @"Surya, the radiant Sun god, rides a golden chariot pulled by seven horses, representing the 
seven colors of sunlight and the seven days of the week. His rays illuminate Earth, nurture life, 
and dispel darkness, ensuring the continuation of cosmic order. Ancient texts describe sunrise 
rituals, offering water and flowers, as a mark of respect for Surya’s journey. Farmers and sages 
tracked his path to determine seasons and agricultural cycles. Surya’s daily movement 
symbolizes dharma and the eternal rhythm of time, teaching that knowledge, light, and moral 
order guide humanity, linking the heavens to human life and the natural world."
            ),

            // 2. Chandra – The Moon God
            new MythologyStory(
                "Hindu Mythology",
                "Chandra – The Moon God",
                @"Chandra, the Moon god, rides a chariot drawn by ten white horses, governing time, tides, 
emotions, and mental clarity. He married Daksha’s 27 daughters, the Nakshatras, showing 
favoritism to Rohini, which angered the other wives. Daksha cursed Chandra, explaining the 
Moon’s waxing and waning. Festivals like Karva Chauth honor him, reflecting his influence on 
human life. The Moon’s gentle light is associated with fertility, calmness, and cycles of 
reflection. Astrology also tracks his position to predict emotions and destiny. Chandra’s myth 
teaches patience, fairness, and harmony between cosmic patterns and human experience, 
showing the interconnectedness of the universe, morality, and daily life."
            ),

            // 3. Rahu & Ketu – The Eclipse Demons
            new MythologyStory(
                "Hindu Mythology",
                "Rahu & Ketu – The Eclipse Demons",
                @"Rahu and Ketu originated from Svarbhanu, who tried to drink the nectar of immortality during 
the churning of the ocean. Vishnu severed him, creating Rahu (head) and Ketu (tail). These 
celestial demons eternally chase the Sun and Moon, causing eclipses. They symbolize karmic 
consequences, the interplay of light and darkness, and the challenges humans face in life. 
Rituals during eclipses reflect respect for cosmic forces and the inevitability of fate. The myth 
emphasizes perseverance, ethical behavior, and understanding that temporary darkness is part 
of life. It blends astronomy, astrology, and moral teachings, showing how celestial events mirror 
human experiences and guide spiritual and ethical growth."
            ),

            // 4. The Saptarishi – The Seven Sages
            new MythologyStory(
                "Hindu Mythology",
                "The Saptarishi – The Seven Sages",
                @"The Saptarishi, consisting of Vashishtha, Vishwamitra, Atri, Gautama, Kashyapa, Bharadwaja, 
and Jamadagni, are immortal sages represented as seven stars in Ursa Major. They embody 
wisdom, morality, and spiritual guidance. Ancient civilizations used them for navigation, 
seasonal calculations, and timing rituals. The sages symbolize the virtues of learning, patience, 
and ethical conduct. Observing the Saptarishi inspires humans to pursue enlightenment and 
moral integrity. Their presence in the night sky demonstrates how knowledge and virtue are 
eternal, bridging the cosmic and earthly realms. The myth teaches that human actions, like 
stars, leave a lasting mark on the universe and reflect the harmony of cosmic order."
            ),

            // 5. Arundhati & Vashishtha – The Eternal Couple
            new MythologyStory(
                "Hindu Mythology",
                "Arundhati & Vashishtha – The Eternal Couple",
                @"Vashishtha and Arundhati, immortalized as the binary stars Mizar and Alcor, symbolize marital 
fidelity, devotion, and virtue. Newlyweds in India are shown these stars to inspire lifelong 
loyalty. Their steady glow represents enduring love, patience, and ethical partnership. 
Observing them reflects moral qualities and emotional harmony, showing that human 
relationships mirror cosmic order. The story emphasizes balance, commitment, and spiritual 
guidance within marriage. By linking human fidelity to celestial patterns, this myth teaches that 
ethical behavior, emotional strength, and companionship are eternal ideals, much like the stars 
that shine unchangingly in the night sky, providing inspiration and moral direction to humanity."
            ),

            // 6. Chandra’s 27 Wives – The Nakshatras
            new MythologyStory(
                "Hindu Mythology",
                "Chandra’s 27 Wives – The Nakshatras",
                @"Chandra married 27 daughters of Daksha, the Nakshatras, each representing a lunar mansion 
influencing human destiny. Favoritism towards Rohini angered the other wives, resulting in 
Daksha cursing the Moon, which explains the waxing and waning phases. These constellations 
became crucial in Hindu astrology, guiding rituals, planting cycles, and personal decisions. 
Observing them connects humans to celestial patterns, morality, and cosmic rhythm. The myth 
teaches the importance of fairness, patience, and respect for others. It integrates astronomy, 
spirituality, and ethical lessons, showing that the movements of the Moon and stars are mirrors 
of human behavior, destiny, and the natural cycles of life on Earth."
            ),

            // 7. Shani – The Planet Saturn
            new MythologyStory(
                "Hindu Mythology",
                "Shani – The Planet Saturn",
                @"Shani, the god of Saturn, rides a crow, representing karma, justice, discipline, and life lessons. 
His slow movement in the sky symbolizes patience and ethical consequences. Astrology tracks 
Shani’s influence to anticipate challenges and guide moral decisions. Devotees perform rituals 
and prayers to seek balance and protection. Shani’s myth teaches that hardships are 
opportunities for growth, perseverance, and adherence to dharma. Observing Saturn 
encourages reflection on personal actions and their repercussions. His story bridges celestial 
observation with moral teachings, reminding humans that cosmic forces reward virtue and 
patience, making the heavens a source of ethical and spiritual guidance."
            ),

            // 8. Mangala – The Planet Mars
            new MythologyStory(
                "Hindu Mythology",
                "Mangala – The Planet Mars",
                @"Mangala, born from Shiva’s energy, represents courage, energy, and martial prowess. As the 
planet Mars, he governs strength, protection, and discipline. Worshippers perform rituals to 
invoke courage, resilience, and skill. His position in astrology influences ambition, decisions, and 
conflict management. Mangala’s story teaches that physical or personal power must be guided 
by ethics and responsibility. It links celestial observation with moral and spiritual behavior, 
showing that the universe mirrors human character and actions. Observing Mars inspires 
bravery and patience, emphasizing the harmonious balance of cosmic energy, ethical conduct, 
and human endeavor."
            ),

            // 9. The Navagraha – Nine Celestial Deities
            new MythologyStory(
                "Hindu Mythology",
                "The Navagraha – Nine Celestial Deities",
                @"The Navagraha, including the Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, and Ketu, 
are celestial deities governing cosmic order and human destiny. Each planet has unique qualities 
affecting morality, health, and success. Rituals and prayers connect humans to these cosmic 
forces. Astrology uses their positions to guide decisions and spiritual growth. Observing the 
Navagraha teaches balance, responsibility, and awareness of natural cycles. The myth illustrates 
how the universe’s movements influence life on Earth, emphasizing harmony between human 
actions, ethics, and celestial patterns, blending astronomy, mythology, and spiritual wisdom into 
a unified framework for life guidance."
            ),

            // 10. Vishnu’s Vamana Avatar
            new MythologyStory(
                "Hindu Mythology",
                "Vishnu’s Vamana Avatar",
                @"Vishnu incarnated as the dwarf Vamana to humble King Bali, who had conquered the three 
worlds. Asking for three steps of land, Vamana expanded to cosmic size, covering Earth and 
heaven in two steps, and placed the third on Bali’s head. This act demonstrates humility, divine 
wisdom, and cosmic balance. The story teaches that even the mighty must respect dharma. 
Vamana bridges human morality with celestial symbolism, illustrating that cosmic and ethical 
laws are intertwined. It reflects the idea that devotion, intelligence, and righteousness maintain 
order, while celestial imagery conveys moral principles through myth and legend."
            ),

            // 11. Dhruva – The Immortal Pole Star
            new MythologyStory(
                "Hindu Mythology",
                "Dhruva – The Immortal Pole Star",
                @"Dhruva, a young prince, was rejected by his father and stepmother, which led him to seek 
Vishnu’s blessings. Through intense meditation and penance, he gained divine favor and was 
placed eternally in the heavens as the Pole Star, around which all other stars revolve. His 
unwavering focus and discipline symbolize perseverance, devotion, and spiritual growth. Dhruva 
became a celestial guide for sailors, astrologers, and devotees, representing constancy, moral 
integrity, and divine grace. The story teaches the power of determination, patience, and faith, 
connecting human effort with cosmic reward, and showing that devotion can make one 
immortal in both spirit and influence."
            ),

            // 12. Helios & Selene – Sun and Moon Riders
            new MythologyStory(
                "Greek & Roman Mythology",
                "Helios & Selene – Sun and Moon Riders",
                @"Helios drives the Sun chariot across the sky, while Selene guides the Moon, siblings representing 
the cycles of day and night. Their eternal movement explains natural rhythms, seasons, and the 
balance of light and darkness. Helios’ golden horses symbolize vitality and life, while Selene’s 
gentle glow governs emotions and time. The myth teaches humans about the importance of 
cosmic order, the passage of time, and respect for natural cycles. Observing them helped early 
societies track days and seasons, integrating astronomy with moral and cultural understanding. 
Their stories bridge human life with celestial phenomena, blending observation and mythology."
            ),

            // 13. Orion – The Hunter in the Stars
            new MythologyStory(
                "Greek & Roman Mythology",
                "Orion – The Hunter in the Stars",
                @"Orion, the mighty hunter, was placed among the stars after being killed by a scorpion sent by 
Gaia. In the night sky, Orion eternally avoids or chases Scorpius, symbolizing courage, pride, and 
skill. Ancient Greeks used his position to track seasons and for navigation. The myth illustrates 
the interplay of human qualities—strength, arrogance, and heroism—with cosmic patterns. 
Orion’s constellation reflects lessons about human behavior, mortality, and perseverance. His 
eternal presence in the sky teaches that deeds, virtues, and vices are mirrored in the cosmos, 
linking mythology, morality, and astronomy."
            ),

            // 14. Andromeda & Perseus – Lovers in the Sky
            new MythologyStory(
                "Greek & Roman Mythology",
                "Andromeda & Perseus – Lovers in the Sky",
                @"Andromeda was chained to a rock as a sacrifice to a sea monster. Perseus rescued her using 
Medusa’s head to turn the beast to stone. Both were immortalized as constellations. The story 
teaches bravery, love, and divine intervention. Observing these stars in the northern sky 
illustrates the interplay between human courage, fate, and the cosmos. It also emphasizes 
loyalty, sacrifice, and moral judgment. Ancient astronomers used the constellations for 
navigation, linking practical observation with myth. Andromeda and Perseus remind humanity 
that celestial patterns preserve stories of heroism, love, and cosmic morality for generations."
            ),

            // 15. The Pleiades – The Seven Sisters
            new MythologyStory(
                "Greek & Roman Mythology",
                "The Pleiades – The Seven Sisters",
                @"The Pleiades, daughters of Atlas and Pleione, were transformed into stars to escape Orion’s 
pursuit. Six stars shine brightly while Merope dims because she married a mortal. The 
constellation guided navigation, agriculture, and seasonal timing in ancient cultures. Their myth 
symbolizes the combination of divine intervention, human emotion, and celestial mapping. 
Observing the Pleiades teaches vigilance, virtue, and the passage of time. The story illustrates 
how humans projected morals, familial bonds, and heroic narratives onto the night sky, merging 
astronomy with myth and daily life."
            ),

            // 16. Venus/Aphrodite – Morning and Evening Star
            new MythologyStory(
                "Greek & Roman Mythology",
                "Venus/Aphrodite – Morning and Evening Star",
                @"Aphrodite, associated with Venus, shines as the Morning and Evening Star. She represents love, 
beauty, desire, and hope. Observing Venus provided guidance for early navigation and 
calendars. The myth demonstrates the connection between celestial observation and human 
ideals, illustrating passion, creativity, and moral reflection. Venus’ dual appearances symbolize 
balance between beginnings and endings, morning and evening, love and duty. Her story 
reflects how ancient civilizations integrated astronomy, morality, and artistry, connecting human 
behavior with cosmic patterns and inspiring admiration and ethical contemplation."
            ),

            // 17. Ra and the Solar Barque
            new MythologyStory(
                "Egyptian Mythology",
                "Ra and the Solar Barque",
                @"Ra sails the sky in his solar barque each day, bringing light, warmth, and life to Earth. By night, 
he journeys through the underworld, battling Apophis, the serpent of chaos, to ensure the Sun 
rises again. This daily cycle represents the struggle between order and chaos, life and death. 
Egyptians aligned temples and festivals with Ra’s journey, emphasizing reverence for cosmic 
rhythm. Observing Ra taught humans about resilience, renewal, and the importance of moral 
and spiritual order. His story illustrates that celestial movements symbolize eternal cycles, 
guiding ethical behavior and societal stability."
            ),

            // 18. Nut – The Sky Goddess
            new MythologyStory(
                "Egyptian Mythology",
                "Nut – The Sky Goddess",
                @"Nut, the sky goddess, arches over Earth, swallowing the Sun each night and birthing it every 
morning. She embodies cosmic cycles, renewal, and divine protection. Her story explains day￾night rhythms, seasons, and celestial order. Egyptians honored Nut to maintain harmony 
between gods and humanity. Nut teaches the importance of balance, continuity, and respect for 
the natural world. Her myth links celestial observation with cultural and spiritual practices, 
demonstrating that the heavens reflect human life and moral responsibility, guiding daily 
actions and understanding of cosmic law."
            ),

            // 19. Sirius & the Dogon People (Mali)
            new MythologyStory(
                "African Mythology",
                "Sirius & the Dogon People (Mali)",
                @"The Dogon people of Mali spoke of Po Tolo, a companion star to Sirius, centuries before 
telescopes discovered it. Sirius guided planting, hunting, and spiritual rituals. Po Tolo’s accurate 
description reflects deep astronomical knowledge. The myth emphasizes how humans used 
celestial observation for survival, moral guidance, and cultural storytelling. It teaches respect for 
the universe, showing that astronomy, mythology, and practical life were deeply intertwined. 
Observing Sirius and Po Tolo provided both literal guidance and symbolic lessons about cosmic 
order, divine wisdom, and the interconnectedness of humans and the stars."
            ),

            // 20. The Cowherd & Weaver Girl (Chinese)
            new MythologyStory(
                "Asian Mythology",
                "The Cowherd & Weaver Girl (Chinese)",
                @"Zhinü (Vega) and Niulang (Altair) fell in love but were separated by the Milky Way. Once a year, 
magpies form a bridge so the lovers can reunite. This myth explains star positions, seasonal 
cycles, and the annual Qixi festival. It conveys loyalty, devotion, and patience, while teaching 
that cosmic forces influence human life. Observing these stars connects morality, cultural 
tradition, and astronomy. The story shows how celestial events and human experiences are 
intertwined, demonstrating love, separation, and reunion as both cosmic phenomena
and ethical lessons."
            ),

            // 21. Amaterasu – The Sun Goddess (Japanese)
            new MythologyStory(
                "Asian Mythology",
                "Amaterasu – The Sun Goddess (Japanese)",
                @"Amaterasu hid in a cave after her brother Susanoo’s violent outburst, plunging the world into 
darkness. The other gods lured her out with celebration, restoring sunlight. Her myth teaches 
the Sun’s essential role, resilience, and harmony. It symbolizes balance, cooperation, and the 
moral necessity of resolving conflict. Observing the Sun’s journey reinforced seasonal 
awareness, spiritual reflection, and societal cohesion. Amaterasu’s story integrates astronomy, 
morality, and cultural values, showing that celestial phenomena embody lessons for human 
behavior and collective well-being."
            ),

            // 22. Sunna & Mani – Norse Sun and Moon Siblings
            new MythologyStory(
                "Norse Mythology",
                "Sunna & Mani – Norse Sun and Moon Siblings",
                @"Sunna (Sól) drives the Sun across the sky, pursued by wolves, while Mani (Máni) guides the 
Moon. Their eternal chase explains day-night cycles, seasons, and the passage of time. The 
myth teaches cosmic balance, inevitability, and natural rhythm. Observing Sunna and Mani 
helped Norse people track seasonal changes, essential for survival. The story conveys lessons 
about perseverance, timing, and respect for natural law. The Sun and Moon as siblings also 
symbolize family, cooperation, and interconnectedness in the cosmos."
            ),

            // 23. The Five Suns – Creation and Renewal (Aztec)
            new MythologyStory(
                "Aztec Mythology",
                "The Five Suns – Creation and Renewal",
                @"The Aztecs believed in five Suns, representing successive world ages. Each Sun ended in 
destruction, teaching humans about impermanence, responsibility, and renewal. The current 
Fifth Sun, Tonatiuh, governs life but requires human offerings to maintain cosmic balance. The 
myth explains natural disasters, societal duty, and the connection between divine and mortal 
realms. Observing celestial patterns guided agriculture, rituals, and governance. The story 
emphasizes respect for cosmic order, moral accountability, and the cyclical nature of existence, 
integrating astronomy, ethics, and mythology."
            ),

            // 24. Matariki – Māori New Year (Pleiades)
            new MythologyStory(
                "Māori Mythology",
                "Matariki – Māori New Year (Pleiades)",
                @"Matariki, the rising of the Pleiades, marks the Māori New Year. The stars represent the eyes of 
Papatūānuku, guiding agricultural planning, remembrance of ancestors, and spiritual reflection. 
Their appearance signifies new beginnings, harvest preparation, and community cohesion. 
Observing Matariki blends astronomy, cultural storytelling, and moral teachings. It illustrates 
how humans use celestial patterns to navigate both practical and spiritual life. The story 
emphasizes the importance of tradition, harmony with nature, and communal responsibility, 
showing how mythology and astronomy integrate to guide human life."
            ),

            // 25. Ishtar – Star of Love and War (Venus)
            new MythologyStory(
                "Mesopotamian Mythology",
                "Ishtar – Star of Love and War (Venus)",
                @"Ishtar, the Morning and Evening Star (Venus), governs love, beauty, fertility, and war. Her 
brightness represents passion, moral decision-making, and human desire. Mesopotamians 
observed Venus to track time, agricultural cycles, and rituals. Her myth teaches balance 
between desire and duty, peace and conflict. Observing Venus guided personal behavior, 
societal planning, and spiritual practice. Ishtar’s story demonstrates how celestial observation, 
mythology, and ethics were intertwined, showing that stars reflect human values, inspire moral 
reflection, and serve as both literal and symbolic guides for society."
            )
        };
    }

    /// <summary>
    /// Returns a random mythology story (type, title, story).
    /// </summary>
    public MythologyStory GetRandomStory()
    {
        if (allStories == null || allStories.Count == 0)
        {
            Debug.LogError("MythologyStoryDatabase: allStories is empty!");
            return null;
        }

        int index = Random.Range(0, allStories.Count);
        return allStories[index];
    }
}
