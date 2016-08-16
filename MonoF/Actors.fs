module Actors
 
open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


type DynamicBody =
    {
        Velocity:Vector2;
        Acceleration:Vector2;
        Friction:float;
        RotationVelocity:float;
        RotationAcceleration:float;
        RotationFriction:float;
    }

type BodyType =
    | Static
    | Dynamic of DynamicBody

type ShootingType =
    {
        Texture : Texture2D;
        IsShooting : bool;
        LastShot : int;
        ShotCooldown : int;
    }

type ActorType =
    | Player of ShootingType
    | Alien of ShootingType
    | Projectile
    | Obstacle
 
type WorldActor =
    {
        ActorType : ActorType;
        Position : Vector2;
        Rotation : float;
        Size : Vector2;
        Texture : Texture2D;
        BodyType : BodyType;
        IsAlive : bool
    }

let currentBounds worldActor = 
    Rectangle((int worldActor.Position.X), (int worldActor.Position.Y), (int worldActor.Size.X), (int worldActor.Size.Y))

let loadImage (device:GraphicsDevice) path =
    use stream = File.OpenRead(path)
    let texture = Texture2D.FromStream(device, stream)
    let textureData = Array.create<Color> (texture.Width * texture.Height) Color.Transparent
    texture.GetData(textureData)
    texture

let CreateActor (device:GraphicsDevice) folderPath (textureName, actorType, position, size, isStatic) :WorldActor =
    let texture = loadImage device (folderPath + textureName)
              
    let bt = if isStatic then
                Static
             else
                Dynamic({Velocity = Vector2(0.f, 0.f) ; Acceleration = Vector2(0.f, 0.f) ; Friction = 0.1 ; RotationVelocity = 0.0 ; RotationAcceleration = 0.0 ; RotationFriction = 0.2})

    { ActorType = actorType; Position = position; Rotation = 0.0; Size = size; Texture = texture; BodyType = bt; IsAlive = true}
   