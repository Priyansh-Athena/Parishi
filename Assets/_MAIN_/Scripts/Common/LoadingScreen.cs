using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup panelGroup;
    public Image loadingImage;
    public TMP_Text waitText;
    public TMP_Text factText;

    [Header("Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float imagePulseScale = 1.05f;
    public float imagePulseDuration = 1.5f;
    public float factChangeInterval = 6f;
    public float waitTextInterval = 0.5f; // time between dot updates

    private bool isAnimating = false;
    private Coroutine factRoutine;
    private Coroutine waitTextRoutine;
    private List<string> spaceFacts = new List<string>();

    void Start()
    {
        panelGroup.alpha = 0f;
        waitText.alpha = 0f;
        factText.alpha = 0f;
        loadingImage.transform.localScale = Vector3.one;
        LoadSpaceFacts();
    }

    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadingSequence(sceneName));
    }

    private IEnumerator LoadingSequence(string sceneName)
    {
        ShowLoadingScreen();

        yield return new WaitForSeconds(5f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        asyncLoad.allowSceneActivation = true;
        HideLoadingScreen();
        yield return new WaitForSeconds(fadeOutDuration);
    }

    public void ShowLoadingScreen()
    {
        panelGroup.blocksRaycasts = true;
        panelGroup.DOFade(1f, fadeInDuration);

        waitText.text = "Please wait";
        waitText.DOFade(1f, fadeInDuration);
        factText.DOFade(1f, fadeInDuration);

        // Animate loading image — rotate clockwise continuously
        loadingImage.transform.DORotate(new Vector3(0, 0, -360f), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // Pulse the image for a bit of life
        loadingImage.transform.DOScale(imagePulseScale, imagePulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        if (!isAnimating)
        {
            factRoutine = StartCoroutine(ChangeFactRoutine());
            waitTextRoutine = StartCoroutine(AnimateWaitText());
        }
    }

    public void HideLoadingScreen()
    {
        if (factRoutine != null) StopCoroutine(factRoutine);
        if (waitTextRoutine != null) StopCoroutine(waitTextRoutine);
        isAnimating = false;

        panelGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            panelGroup.blocksRaycasts = false;
            loadingImage.transform.DOKill();
        });
    }

    private IEnumerator AnimateWaitText()
    {
        isAnimating = true;
        string baseText = "Please wait";
        int dotCount = 0;

        while (isAnimating)
        {
            waitText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // cycles 0–3 dots
            yield return new WaitForSeconds(waitTextInterval);
        }
    }

    public void ChangeRandomFact()
    {
        if (spaceFacts.Count == 0) return;

        string newFact = spaceFacts[Random.Range(0, spaceFacts.Count)];

        factText.DOFade(0f, 0.4f).OnComplete(() =>
        {
            factText.text = newFact;
            factText.DOFade(1f, 0.4f);
        });
    }

    private IEnumerator ChangeFactRoutine()
    {
        isAnimating = true;
        ChangeRandomFact();

        while (isAnimating)
        {
            yield return new WaitForSeconds(factChangeInterval);
            ChangeRandomFact();
        }
    }

    private void LoadSpaceFacts()
    {
        // 100+ astrophysics and space facts
        spaceFacts.AddRange(new string[]
        {
            "Light from the Sun takes 8 minutes and 20 seconds to reach Earth.",
            "The Milky Way galaxy is about 100,000 light-years wide.",
            "A day on Venus is longer than a year on Venus.",
            "Neutron stars can spin up to 600 times per second.",
            "Jupiter has more than 90 known moons.",
            "Black holes can warp space and time.",
            "Saturn could float in water due to its low density.",
            "The hottest planet in the solar system is Venus.",
            "The Sun accounts for 99.86% of the solar system’s mass.",
            "There are more stars in the universe than grains of sand on all Earth's beaches.",
            "Astronauts grow taller in space due to spinal decompression.",
            "A teaspoon of neutron star material would weigh about 6 billion tons.",
            "Mars has the tallest volcano in the solar system: Olympus Mons.",
            "The Great Red Spot on Jupiter is a storm that’s lasted for centuries.",
            "One year on Mercury equals 88 Earth days.",
            "A year on Neptune lasts 165 Earth years.",
            "The first black hole ever photographed was M87* in 2019.",
            "Space is completely silent — there’s no air to carry sound.",
            "Comets are mostly made of ice and dust.",
            "The Andromeda Galaxy is moving toward the Milky Way.",
            "Pluto is smaller than Earth’s Moon.",
            "There are galaxies that contain trillions of stars.",
            "Some planets may have diamond rain.",
            "Saturn’s rings are made mostly of water ice.",
            "The Sun will eventually become a white dwarf.",
            "A light-year is nearly 9.46 trillion kilometers.",
            "Time passes slower near strong gravity — like near a black hole.",
            "The Moon is drifting away from Earth by about 3.8 cm per year.",
            "Mars once had liquid water on its surface.",
            "A day on Jupiter lasts only 10 hours.",
            "Venus rotates in the opposite direction to most planets.",
            "There could be more than 100 billion galaxies in the observable universe.",
            "The universe is about 13.8 billion years old.",
            "Some stars explode in supernovae at the end of their lives.",
            "Gamma-ray bursts are the most powerful explosions in the universe.",
            "The largest known star, UY Scuti, is about 1,700 times the Sun’s radius.",
            "Black holes can merge and release massive gravitational waves.",
            "White dwarfs are the remnants of stars like our Sun.",
            "Neutron stars are only about 20 km wide but extremely dense.",
            "Dark matter makes up about 27% of the universe.",
            "Dark energy accounts for around 68% of the universe.",
            "The remaining 5% is normal, visible matter.",
            "Our galaxy is on a collision course with Andromeda in 4 billion years.",
            "The Hubble Space Telescope orbits Earth every 97 minutes.",
            "Voyager 1 is the farthest human-made object from Earth.",
            "Mercury has almost no atmosphere.",
            "The Sun’s core temperature is about 15 million °C.",
            "The speed of light is 299,792 kilometers per second.",
            "Some black holes emit jets faster than 99% the speed of light.",
            "Jupiter’s magnetic field is 14 times stronger than Earth’s.",
            "Saturn’s moon Titan has lakes of liquid methane.",
            "Uranus rotates on its side — it’s tilted by about 98 degrees.",
            "Solar flares release more energy than 1 million atomic bombs.",
            "There are stars that are colder than the human body.",
            "The center of the Milky Way hosts a supermassive black hole called Sagittarius A*.",
            "Exoplanets are planets that orbit other stars.",
            "We’ve discovered over 5,000 exoplanets so far.",
            "The largest volcano in the solar system is on Mars.",
            "A single bolt of lightning can heat the air to 30,000 °C.",
            "Our solar system takes about 230 million years to orbit the galaxy.",
            "Pulsars are rotating neutron stars that emit radio waves.",
            "There’s a giant cloud of alcohol floating in space.",
            "Space smells like burnt metal and seared steak (according to astronauts).",
            "Some planets may have iron rain.",
            "Saturn’s moon Enceladus shoots geysers of water into space.",
            "Astronauts’ hearts become rounder in space.",
            "Spacecraft must travel over 11 km/s to escape Earth’s gravity.",
            "The Sun converts 4 million tons of matter into energy every second.",
            "The James Webb Space Telescope can see back in time billions of years.",
            "The cosmic microwave background is the afterglow of the Big Bang.",
            "Most galaxies have supermassive black holes at their centers.",
            "The Moon has moonquakes caused by tidal forces.",
            "In space, metals can weld together without heat or pressure.",
            "A year on Pluto lasts 248 Earth years.",
            "Some stars are so dense a teaspoon would weigh tons.",
            "The edge of the observable universe is about 46 billion light-years away.",
            "Jupiter’s moon Europa may have an ocean beneath its ice.",
            "Mars appears red because of iron oxide (rust) on its surface.",
            "Neptune’s winds can reach speeds over 2,000 km/h.",
            "There are rogue planets not bound to any star.",
            "Space tourism is already happening.",
            "A black hole’s gravity can bend light itself.",
            "There’s a hexagonal storm on Saturn’s north pole.",
            "Earth’s magnetic field protects us from solar radiation.",
            "Some quasars shine brighter than entire galaxies.",
            "Gravity on Mars is only 38% that of Earth.",
            "The Sun orbits the Milky Way at about 828,000 km/h.",
            "Some stars can live for trillions of years.",
            "The first exoplanet was discovered in 1992.",
            "Every second, a star dies somewhere in the universe.",
            "Earth’s atmosphere extends farther than the Moon’s orbit (tenuously).",
            "There are radio signals that have no known origin.",
            "Astronomers estimate 2 trillion galaxies in the observable universe.",
            "Some stars are blue, white, yellow, or red depending on temperature.",
            "Cosmic rays constantly hit Earth from outer space.",
            "The universe is expanding faster over time.",
            "The first human in space was Yuri Gagarin in 1961.",
            "Black holes don’t “suck” — they just have immense gravity.",
            "A solar eclipse happens when the Moon blocks the Sun.",
            "A lunar eclipse happens when Earth’s shadow covers the Moon."
        });
    }
}
