# Bhag Milkha Bhage - 3D Endless Runner Game

## Game Overview
**Bhag Milkha Bhage** is a 3D endless runner mobile game inspired by Subway Surfers, featuring an Indian-themed environment where players control a young runner being chased by a dangerous don.

---

## 📁 Project Structure

```
BhagMilkhaBhage/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs          # Main game state controller
│   │   ├── PlayerController.cs     # Player movement & controls
│   │   ├── ObstacleSpawner.cs      # Obstacle & coin spawning
│   │   ├── GameComponents.cs       # Coin, PowerUp, Camera, etc.
│   │   └── UIManager.cs            # UI screens & menus
│   ├── Prefabs/                    # Game object prefabs
│   ├── Materials/                  # Material assets
│   ├── Scenes/                     # Unity scenes
│   ├── Audio/                      # Sound effects & music
│   └── Textures/                   # Texture assets
└── ProjectSettings/                # Unity project settings
```

---

## 🎮 Core Features

### Gameplay Mechanics
- **Automatic Forward Movement**: Character runs continuously
- **Three Lane System**: Left, Middle, Right lanes
- **Swipe Controls**:
  - Swipe Left/Right → Change lanes
  - Swipe Up → Jump over obstacles
  - Swipe Down → Slide under barriers
- **Progressive Difficulty**: Speed increases over time
- **Don Chase Mechanic**: Getting hit brings the villain closer

### Environment
- Indian-themed locations (railway tracks, streets, markets, villages)
- Dynamic obstacles: trains, cars, barricades, animals, carts
- Day/night cycle with lighting changes
- Parallax scrolling backgrounds

### Power-Ups
1. **Speed Boost**: Temporary sprint (1.5x speed)
2. **Shield**: Protects from one crash
3. **Coin Magnet**: Attracts nearby coins
4. **Double Score**: 2x score multiplier

### Rewards System
- Collect coins during runs
- Unlock characters and outfits
- Daily missions and challenges
- High score tracking

---

## 🔧 Setup Instructions

### 1. Unity Project Setup
1. Open Unity Hub
2. Create new 3D project named "BhagMilkhaBhage"
3. Copy all files from this folder into your Unity project's Assets folder

### 2. Scene Setup

#### Create Main Scene:
1. Create empty GameObject named **"GameManager"**
   - Attach `GameManager` script
   - Assign UI references in Inspector

2. Create Player:
   - Create 3D Capsule or import character model
   - Add `CharacterController` component
   - Add `PlayerController` script
   - Tag as **"Player"**
   - Add `Animator` component with running/jumping/sliding animations

3. Create Spawner:
   - Create empty GameObject named **"ObstacleSpawner"**
   - Attach `ObstacleSpawner` script
   - Assign obstacle prefabs

4. Create Camera:
   - Position Main Camera behind player
   - Attach `CameraFollow` script
   - Set target to player

5. Create UI Canvas:
   - Create Canvas (Screen Space - Overlay)
   - Add panels for: Main Menu, Game HUD, Pause, Game Over
   - Attach `UIManager` script
   - Connect all UI element references

### 3. Layer Setup
Create these layers in Unity:
- **Player** (Layer 6)
- **Obstacle** (Layer 7)
- **Coin** (Layer 8)
- **Environment** (Layer 9)

### 4. Tag Setup
Create these tags:
- **Player**
- **Obstacle**
- **Coin**
- **PowerUp**
- **Ground**

### 5. Create Obstacle Prefabs

#### Low Obstacles (Jump over):
- Barriers, crates, small animals
- Height: ~0.5-1m
- Add BoxCollider, tag as "Obstacle"

#### High Obstacles (Slide under):
- Overhead signs, low bridges
- Position Y: 1.2m
- Add BoxCollider, tag as "Obstacle"

#### Full Obstacles (Dodge):
- Trains, walls, large vehicles
- Full height coverage
- Add BoxCollider, tag as "Obstacle"

#### Moving Obstacles:
- Cars, trains moving across lanes
- Add `MovingObstacle` script
- Add BoxCollider, tag as "Obstacle"

### 6. Create Coin Prefab
1. Create Cylinder or use coin model
2. Scale: (0.5, 0.5, 0.05)
3. Add rotating material (gold color)
4. Add `Coin` script
5. Add SphereCollider (IsTrigger = true)
6. Tag as "Coin"

### 7. Create Power-Up Prefabs
Create 4 variants:
- **SpeedBoost**: Red orb/capsule
- **Shield**: Blue sphere with glow
- **CoinMagnet**: Purple magnet shape
- **DoubleScore**: Green star/gem

Add `PowerUp` script to each, set type accordingly.

---

## 🎯 Script Configuration Guide

### GameManager Settings
```csharp
initialSpeed: 5.0           // Starting speed
maxSpeed: 15.0              // Maximum speed cap
speedIncreaseRate: 0.1      // Speed increase per second
donMaxDistance: 10.0        // Initial distance from don
donCatchUpSpeed: 2.0        // How fast don approaches on hit
```

### PlayerController Settings
```csharp
laneDistance: 3.0           // Distance between lanes
laneSwitchSpeed: 10.0       // Lane change smoothness
jumpForce: 8.0              // Jump height
slideDuration: 0.8          // Slide duration in seconds
minSwipeDistance: 50        // Minimum swipe pixels
```

### ObstacleSpawner Settings
```csharp
spawnDistance: 50.0         // Spawn ahead of player
minSpawnInterval: 1.0       // Fastest spawn rate
maxSpawnInterval: 2.5       // Slowest spawn rate
coinSpawnChance: 0.7        // 70% chance for coins
powerUpSpawnChance: 0.15    // 15% chance for power-ups
```

---

## 🎨 Art & Asset Recommendations

### Character Models
- **Main Character**: Athletic Indian youth (sports outfit)
- **Villain (Don)**: Menacing figure with aggressive stance
- **Unlockable Skins**:
  - Sports outfit (default)
  - Traditional kurta
  - Army camouflage
  - Street wear
  - Festival special

### Environment Assets
- Railway tracks with sleepers
- Indian street elements (chai stalls, rickshaws)
- Market stalls with colorful fabrics
- Village huts and trees
- Temple architecture elements
- Bollywood-style billboards

### Obstacle Variations
- Stationary: Crates, barrels, construction barriers
- Moving: Auto-rickshaws, cows, bicycles, trains
- Environmental: Low hanging signs, overhead wires

### UI Design
- Colorful, vibrant Indian theme
- Saffron, green, and white color scheme
- Bold, readable fonts
- Animated buttons with feedback

---

## 🎵 Audio Implementation

### Sound Effects Needed:
- Footsteps (running on different surfaces)
- Jump sound
- Slide/duck sound
- Coin collection (pleasant chime)
- Power-up activation
- Crash/collision sound
- Don approaching (tense music cue)
- Game over sound

### Background Music:
- Energetic chase theme (main gameplay)
- Tense music (when don is close)
- Victory fanfare (new high score)
- Calm menu music

---

## 📱 Mobile Optimization

### Performance Tips:
1. **Object Pooling**: Reuse obstacles instead of instantiating/destroying
2. **LOD (Level of Detail)**: Reduce polygon count for distant objects
3. **Occlusion Culling**: Hide objects not visible to camera
4. **Texture Compression**: Use ASTC for Android, PVRTC for iOS
5. **Shadow Quality**: Use soft shadows only for nearby objects
6. **Batching**: Static batching for environment, dynamic for characters

### Touch Controls:
- Implement multi-touch support
- Add visual feedback for swipes
- Include sensitivity settings
- Support both portrait and landscape modes

---

## 🚀 Building for Mobile

### Android Build:
1. File → Build Settings → Switch to Android
2. Set Minimum API Level: Android 7.0 (API 24)
3. Enable IL2CPP scripting backend
4. Set ARMv7 and ARM64 architectures
5. Configure player settings (icon, splash screen)
6. Build APK or AAB

### iOS Build:
1. File → Build Settings → Switch to iOS
2. Set Minimum iOS Version: 12.0
3. Configure bundle identifier
4. Set device orientation (landscape)
5. Build Xcode project
6. Archive and upload to App Store

---

## 📋 Testing Checklist

- [ ] Swipe controls responsive on mobile
- [ ] Keyboard controls work in editor (for testing)
- [ ] All obstacle types spawn correctly
- [ ] Coins and power-ups collect properly
- [ ] Shield protects from one hit
- [ ] Speed increases gradually
- [ ] Don gets closer on collision
- [ ] Game over triggers when caught
- [ ] High score saves correctly
- [ ] UI updates in real-time
- [ ] Day/night cycle functions
- [ ] No frame rate drops
- [ ] Memory usage stable

---

## 🔮 Future Enhancements

1. **Multiplayer Mode**: Real-time races with friends
2. **Daily Challenges**: Special missions with rewards
3. **Achievement System**: Google Play Games / Game Center integration
4. **In-App Purchases**: Buy coins, unlock premium characters
5. **Leaderboards**: Global and friend rankings
6. **Special Events**: Festival-themed levels and rewards
7. **Character Abilities**: Unique powers for each character
8. **Vehicle Sections**: Ride motorcycles, bicycles temporarily
9. **Combo System**: Reward skillful consecutive moves
10. **Photo Mode**: Capture and share cool moments

---

## 📄 License & Credits

This game structure is provided as a template for educational purposes. Customize and expand upon it to create your unique endless runner experience.

**Inspired by**: Subway Surfers, Temple Run, and Indian cinema culture

---

## 🆘 Troubleshooting

### Common Issues:

**Player falls through ground:**
- Check CharacterController center and height
- Ensure ground has proper collider
- Verify ground layer assignment

**Obstacles not spawning:**
- Check array assignments in ObstacleSpawner
- Verify spawn distance isn't too far
- Ensure singleton instances exist

**Swipe not working on mobile:**
- Increase minSwipeDistance if too sensitive
- Decrease if not registering swipes
- Test on actual device (simulator may differ)

**Performance issues:**
- Reduce pool size if memory heavy
- Lower shadow quality
- Reduce draw distance
- Use profiler to identify bottlenecks

---

Happy Coding! 🎮🇮🇳
