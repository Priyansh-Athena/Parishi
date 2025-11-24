using System.Collections.Generic;
using UnityEngine;
using QuizSystem.SO;
using UnityEngine.Events;

public class QuizDataLoader : MonoBehaviour
{
    [Header("Assign Quiz Data SO Here")]
    public QuizData quizData;

    [Header("Events")]
    public UnityEvent OnQuestionsLoaded;

    // Raw question data structure
    private class RawQuestion
    {
        public string question;
        public string[] options;
        public int correctIndex;

        public RawQuestion(string q, string[] o, int c)
        {
            question = q;
            options = o;
            correctIndex = c;
        }
    }

    // ---------------------------------------------------
    //  ALL 50 QUESTIONS FROM PDF  (with correct answers)
    // ---------------------------------------------------
    private List<RawQuestion> allQuestions = new List<RawQuestion>()
    {
        new RawQuestion("The Sun is classified as a _______.",
            new[]{"Planet","Star","Satellite","Asteroid"}, 1),
        new RawQuestion("The planet closest to the Sun is _______.",
            new[]{"Venus","Mercury","Earth","Mars"}, 1),
        new RawQuestion("Which planet is known as the “Red Planet”?",
            new[]{"Mercury","Venus","Mars","Jupiter"}, 2),
        new RawQuestion("The only planet known to support life is _______.",
            new[]{"Earth","Mars","Venus","Jupiter"}, 0),
        new RawQuestion("The largest planet in our Solar System is _______.",
            new[]{"Earth","Saturn","Neptune","Jupiter"}, 3),
        new RawQuestion("Which planet is known for its beautiful rings?",
            new[]{"Uranus","Saturn","Jupiter","Neptune"}, 1),
        new RawQuestion("The smallest planet in the Solar System is _______.",
            new[]{"Mercury","Venus","Mars","Pluto"}, 0),
        new RawQuestion("A group of stars forming a pattern is called a _______.",
            new[]{"Galaxy","Nebula","Constellation","Meteor"}, 2),
        new RawQuestion("Which planet is known as the “Morning Star” or “Evening Star”?",
            new[]{"Mars","Venus","Jupiter","Saturn"}, 1),
        new RawQuestion("The Milky Way is a type of _______.",
            new[]{"Star","Planet","Galaxy","Nebula"}, 2),
        new RawQuestion("The nearest star to Earth (after the Sun) is _______.",
            new[]{"Sirius","Proxima Centauri","Vega","Betelgeuse"}, 1),
        new RawQuestion("The “Great Bear” is also known as _______.",
            new[]{"Orion","Ursa Major","Cassiopeia","Leo"}, 1),
        new RawQuestion("The Sun gets its energy from _______.",
            new[]{"Nuclear fusion","Nuclear fission","Combustion","Electricity"}, 0),
        new RawQuestion("The Pole Star (Polaris) is found in which constellation?",
            new[]{"Ursa Major","Ursa Minor","Orion","Leo"}, 1),
        new RawQuestion("The Sun’s energy mainly reaches Earth through _______.",
            new[]{"Conduction","Convection","Radiation","Reflection"}, 2),
        new RawQuestion("Jupiter’s Great Red Spot is a _______.",
            new[]{"Volcano","Mountain","Storm","Crater"}, 2),
        new RawQuestion("The planet with the fastest rotation is _______.",
            new[]{"Earth","Jupiter","Saturn","Mars"}, 1),
        new RawQuestion("Which planet currently has the most confirmed moons?",
            new[]{"Earth","Jupiter","Saturn","Neptune"}, 1),
        new RawQuestion("The planet tilted so much that it rotates on its side is _______.",
            new[]{"Uranus","Saturn","Neptune","Mercury"}, 0),
        new RawQuestion("Earth’s twin in size and mass is _______.",
            new[]{"Venus","Mars","Mercury","Neptune"}, 0),
        new RawQuestion("The color of a star depends on its _______.",
            new[]{"Distance","Size","Temperature","Age"}, 2),
        new RawQuestion("Which of the following is the brightest star in the night sky?",
            new[]{"Sirius","Polaris","Rigel","Arcturus"}, 0),
        new RawQuestion("The Sun belongs to which spectral class?",
            new[]{"M","F","G","O"}, 2),
        new RawQuestion("What is the ultimate fate of our Sun?",
            new[]{"Neutron star","Black hole","White dwarf","Supernova"}, 2),
        new RawQuestion("A dying massive star that explodes is called a _______.",
            new[]{"Supernova","Pulsar","Quasar","Nebula"}, 0),
        new RawQuestion("The Orion constellation is also called the _______.",
            new[]{"Archer","Hunter","Scorpion","Lion"}, 1),
        new RawQuestion("The constellation Leo represents a _______.",
            new[]{"Bull","Lion","Goat","Dog"}, 1),
        new RawQuestion("Gemini is represented by _______.",
            new[]{"The Twins","The Crab","The Archer","The Goat"}, 0),
        new RawQuestion("The constellation that resembles a “W” shape is _______.",
            new[]{"Orion","Cassiopeia","Pegasus","Gemini"}, 1),
        new RawQuestion("The constellation Taurus is also known as the _______.",
            new[]{"Lion","Bull","Fish","Ram"}, 1),
        new RawQuestion("What is a star’s life cycle primarily determined by?",
            new[]{"Its age","Its mass","Its color","Its position"}, 1),
        new RawQuestion("What remains after a supernova of a very massive star?",
            new[]{"Red giant","Black hole","White dwarf","Planetary nebula"}, 1),
        new RawQuestion("The Sun’s core temperature is approximately _______.",
            new[]{"1,000°C","15 million°C","100,000°C","1 billion°C"}, 1),
        new RawQuestion("Which type of star is the hottest?",
            new[]{"Red","Yellow","Blue","White"}, 2),
        new RawQuestion("The coldest planet in the Solar System is _______.",
            new[]{"Mars","Neptune","Uranus","Pluto"}, 2),
        new RawQuestion("What are planets that orbit other stars called?",
            new[]{"Dwarf planets","Asteroids","Exoplanets","Meteoroids"}, 2),
        new RawQuestion("The universe began with the _______.",
            new[]{"Supernova","Big Bang","Solar flare","Meteor strike"}, 1),
        new RawQuestion("The age of the universe is estimated to be about _______.",
            new[]{"1 billion years","4.5 billion years","13.8 billion years","100 million years"}, 2),
        new RawQuestion("Which of these is a spiral galaxy?",
            new[]{"Milky Way","Andromeda","Both","None"}, 2),
        new RawQuestion("What holds galaxies together?",
            new[]{"Electromagnetism","Gravity","Radiation","Dark matter only"}, 1),
        new RawQuestion("Black holes have extremely strong _______.",
            new[]{"Magnetic fields","Gravity","Radiation","Light"}, 1),
        new RawQuestion("The boundary around a black hole is called the _______.",
            new[]{"Event horizon","Photon zone","Gravity belt","Space warp"}, 0),
        new RawQuestion("The study of the universe is called _______.",
            new[]{"Biology","Geology","Astronomy","Cosmology"}, 3),
        new RawQuestion("The second largest galaxy nearest to the Milky Way is _______.",
            new[]{"Triangulum","Andromeda","Virgo","Pegasus"}, 1),
        new RawQuestion("Dark energy is believed to cause the universe to _______.",
            new[]{"Shrink","Expand faster","Stop expanding","Collapse"}, 1),
        new RawQuestion("The brightness of a star as seen from Earth is called its _______.",
            new[]{"Luminosity","Apparent magnitude","Color","Density"}, 1),
        new RawQuestion("A galaxy that is spiral in shape is _______.",
            new[]{"Milky Way","Pegasus","Virgo","Triangulum"}, 0),
        new RawQuestion("The constellation containing Sirius is _______.",
            new[]{"Orion","Canis Major","Ursa Major","Leo"}, 1),
        new RawQuestion("The constellation Scorpius represents a _______.",
            new[]{"Lion","Hunter","Scorpion","Archer"}, 2),
        new RawQuestion("The ultimate fate of a supermassive star is _______.",
            new[]{"Planet","Black hole (or neutron star)","White dwarf","Nebula"}, 1),
    };

    // ---------------------------------------------------
    //                LOAD 5 RANDOM QUESTIONS
    // ---------------------------------------------------
    void Start()
    {
        if (quizData == null)
        {
            Debug.LogError("❌ QuizData not assigned!");
            return;
        }
    }

    public void LoadRandomFive()
    {
        // Clear previous entries
        quizData.questions = new List<Question>();

        // Copy list for safe random selection
        List<RawQuestion> pool = new List<RawQuestion>(allQuestions);

        for (int i = 0; i < 5; i++)
        {
            int index = Random.Range(0, pool.Count);
            RawQuestion rq = pool[index];
            pool.RemoveAt(index);

            Question q = new Question();
            q.questionType = QuestionType.Text;
            q.questionText = rq.question;

            q.answerType = AnswerType.Text;
            q.textAnswer = new TextAnswerList();
            q.textAnswer.answers = new List<TextAnswer>();

            // Add answers
            for (int j = 0; j < rq.options.Length; j++)
            {
                q.textAnswer.answers.Add(new TextAnswer
                {
                    answerValue = rq.options[j],
                    isCorrect = (j == rq.correctIndex)
                });
            }

            // Add 60 sec timer
            q.hasTime = true;
            q.questionTimerData = new QuestionTimerData
            {
                timerType = TimerType.Seconds,
                timerValue = 60f
            };

            quizData.questions.Add(q);
        }

        OnQuestionsLoaded?.Invoke();
        Debug.Log("✔ Quiz Data updated with *5 random questions* from the PDF!");
    }
}
