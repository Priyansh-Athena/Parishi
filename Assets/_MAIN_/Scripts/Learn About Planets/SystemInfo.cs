using UnityEngine;
using System.Collections.Generic;

public class SystemInfo : MonoBehaviour
{
    [System.Serializable]
    public class SystemData
    {
        public string systemName;

        [Header("Basic Info")]
        public string type;                     // e.g. Planetary System, Star System
        public string mainBody;                 // e.g. Sun, Earth, etc.
        public string numberOfPlanets;          // e.g. "8 Planets"
        public string keyBodies;                // e.g. "Earth, Mars, Jupiter..."

        [Header("Physical Data")]
        public string centralObjectType;        // e.g. "Star", "Planet"
        public string size;                     // e.g. Diameter or Radius
        public string mass;
        public string temperature;
        public string rotationPeriod;
        public string revolutionPeriod;

        [Header("Discovery Info")]
        public string discoveryYear;
        public string discoveredBy;

        [Header("Fun Fact")]
        [TextArea(2, 5)]
        public string funFact;

        [Header("Special Feature")]
        [TextArea(2, 5)]
        public string specialFeature;
    }

    public InfoFields infoFieldPrefab;
    public Transform infoFieldParent;
    public InfoFields[] infoFields;
    public List<SystemData> systems = new List<SystemData>();

    void Start()
    {
        systems = new List<SystemData>
        {
            // 🌞 Solar System
            new SystemData
            {
                systemName = "Solar System",
                type = "Star System",
                mainBody = "Sun",
                numberOfPlanets = "8 Planets",
                keyBodies = "Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune",
                centralObjectType = "G-Type Main-Sequence Star (Yellow Dwarf)",
                size = "Diameter: ~1.39 million km",
                mass = "1.989 × 10³⁰ kg",
                temperature = "Surface: ~5,500°C, Core: ~15 million°C",
                rotationPeriod = "25 days (approx.)",
                revolutionPeriod = "N/A",
                discoveryYear = "Known since ancient times",
                discoveredBy = "Ancient Astronomers",
                funFact = "The Sun accounts for 99.8% of the total mass of the Solar System.",
                specialFeature = "Contains all major planets, dwarf planets, asteroids, and comets orbiting the Sun."
            },

            // 🌍 Earth System
            new SystemData
            {
                systemName = "Earth System",
                type = "Planetary System",
                mainBody = "Earth",
                numberOfPlanets = "1 (Earth)",
                keyBodies = "Earth and its Moon",
                centralObjectType = "Planet",
                size = "Diameter: 12,742 km",
                mass = "5.97 × 10²⁴ kg",
                temperature = "Average Surface: 15°C",
                rotationPeriod = "24 hours",
                revolutionPeriod = "365.25 days",
                discoveryYear = "N/A (Home planet)",
                discoveredBy = "N/A",
                funFact = "Earth is the only known planet to support life.",
                specialFeature = "Has one large moon and abundant liquid water on its surface."
            },

            // 🔴 Mars System
            new SystemData
            {
                systemName = "Mars System",
                type = "Planetary System",
                mainBody = "Mars",
                numberOfPlanets = "1 (Mars)",
                keyBodies = "Mars, Phobos, Deimos",
                centralObjectType = "Planet",
                size = "Diameter: 6,779 km",
                mass = "6.42 × 10²³ kg",
                temperature = "Average Surface: -60°C",
                rotationPeriod = "24.6 hours",
                revolutionPeriod = "687 Earth days",
                discoveryYear = "Known since ancient times",
                discoveredBy = "Ancient Astronomers",
                funFact = "Mars has the largest volcano in the Solar System — Olympus Mons.",
                specialFeature = "Hosts two small irregular moons — Phobos and Deimos."
            },

            // 🪐 Jupiter System
            new SystemData
            {
                systemName = "Jupiter System",
                type = "Planetary System",
                mainBody = "Jupiter",
                numberOfPlanets = "1 (Jupiter)",
                keyBodies = "Jupiter, 95 Moons (Io, Europa, Ganymede, Callisto...)",
                centralObjectType = "Gas Giant",
                size = "Diameter: 139,820 km",
                mass = "1.90 × 10²⁷ kg",
                temperature = "Cloud tops: -108°C",
                rotationPeriod = "9.9 hours",
                revolutionPeriod = "12 Earth years",
                discoveryYear = "Known since ancient times",
                discoveredBy = "Ancient Astronomers",
                funFact = "Jupiter’s magnetic field is 20,000 times stronger than Earth’s.",
                specialFeature = "Has a massive storm called the Great Red Spot and 4 major Galilean moons."
            },

            // 💍 Saturn System
            new SystemData
            {
                systemName = "Saturn System",
                type = "Planetary System",
                mainBody = "Saturn",
                numberOfPlanets = "1 (Saturn)",
                keyBodies = "Saturn, 146 Moons, Prominent Rings",
                centralObjectType = "Gas Giant",
                size = "Diameter: 116,460 km",
                mass = "5.68 × 10²⁶ kg",
                temperature = "Cloud tops: -138°C",
                rotationPeriod = "10.7 hours",
                revolutionPeriod = "29 Earth years",
                discoveryYear = "Known since ancient times",
                discoveredBy = "Ancient Astronomers",
                funFact = "Saturn’s rings are mostly made of ice and dust particles.",
                specialFeature = "Has the most extensive and visible ring system in the Solar System."
            },

            // 🌀 Uranus System
            new SystemData
            {
                systemName = "Uranus System",
                type = "Planetary System",
                mainBody = "Uranus",
                numberOfPlanets = "1 (Uranus)",
                keyBodies = "Uranus, 27 Moons, Faint Ring System",
                centralObjectType = "Ice Giant",
                size = "Diameter: 50,724 km",
                mass = "8.68 × 10²⁵ kg",
                temperature = "Average: -195°C",
                rotationPeriod = "17.2 hours",
                revolutionPeriod = "84 Earth years",
                discoveryYear = "1781",
                discoveredBy = "William Herschel",
                funFact = "Uranus rotates on its side — its axis tilt is about 98°.",
                specialFeature = "Unique sideways rotation, leading to extreme seasonal variations."
            }
        };

        Setup(0);
    }

    public void Setup(int idx)
    {
        SystemData data = systems[idx];

        // Assign each field from the existing list in the Inspector
        infoFields[0].SetData("System Name", data.systemName);
        infoFields[1].SetData("Type", data.type);
        infoFields[2].SetData("Main Body", data.mainBody);
        infoFields[3].SetData("Number of Planets", data.numberOfPlanets);
        infoFields[4].SetData("Key Bodies", data.keyBodies);
        infoFields[5].SetData("Central Object Type", data.centralObjectType);
        infoFields[6].SetData("Size", data.size);
        infoFields[7].SetData("Mass", data.mass);
        infoFields[8].SetData("Temperature", data.temperature);
        infoFields[9].SetData("Rotation Period", data.rotationPeriod);
        infoFields[10].SetData("Revolution Period", data.revolutionPeriod);
        infoFields[11].SetData("Discovery Year", data.discoveryYear);
        infoFields[12].SetData("Discovered By", data.discoveredBy);
        infoFields[13].SetData("Fun Fact", data.funFact);
        infoFields[14].SetData("Special Feature", data.specialFeature);
    }

    public void IncreaseXP()
    {
        GameManager.Instance.AddXP(25);
    }
}
