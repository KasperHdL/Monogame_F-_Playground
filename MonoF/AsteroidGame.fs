module AsteroidGame

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Actors
open PlayerInput


type AsteroidGame() as game =
    inherit Game()

    let graphics = new GraphicsDeviceManager(game)

    

    let mutable spriteBatch : SpriteBatch = null
           
    let mutable actors = [ (*textureName, actorType, position, size, isStatic*) ]
   
  
    override game.LoadContent() =
        spriteBatch <- new SpriteBatch(game.GraphicsDevice)
        let path = Path.Combine(__SOURCE_DIRECTORY__, "Content/")
        let CreateActor' = CreateActor game.GraphicsDevice path

        let shotTex = loadImage game.GraphicsDevice (path + "shot.png")
        let stdShotType = {IsShooting = false ; Texture = shotTex; LastShot = 0; ShotCooldown = 1000} 

        actors <- [
                (CreateActor' ("ship.png", Player({stdShotType with ShotCooldown = 250}), Vector2(100.f, 100.f), Vector2(64.f,64.f), false));
                (CreateActor' ("alien.png", Alien(stdShotType), Vector2(100.f, 150.f), Vector2(64.f,64.f), false));
            ] @ actors

    
    override game.Update (gameTime) =
        let dt = float32 gameTime.ElapsedGameTime.Milliseconds / float32 1000.0
        let newActors = ref [];

        //Input
        actors <- actors
            |> List.map (fun a ->
                match a.ActorType with
                | Player(s) -> 
                    HandleInput (Keyboard.GetState()) a
                | _ -> a
            )

        //Shooting
        actors <- actors
            |> List.map (fun a ->
                match a.ActorType with
                | Player(s) -> 
                    if s.IsShooting then
                        let t = int gameTime.TotalGameTime.TotalMilliseconds
                        let next = s.LastShot + s.ShotCooldown

                        let shotTime = 
                            if next < t then
                                let aBody = match a.BodyType with
                                            | Dynamic(d) -> d
                                            | Static -> {Velocity = Vector2(0.f, 0.f) ; Acceleration = Vector2(0.f, 0.f) ; Friction = 0.0 ; RotationVelocity = 0.0 ; RotationAcceleration = 0.0 ; RotationFriction = 0.0}

                                let vel = Vector2.Transform(Vector2(0.f, -1000.f), Matrix.CreateRotationZ(float32 a.Rotation))
                                let shotBody = {aBody with Velocity = vel; Friction = float 0.0}
                                let shot = { ActorType = Projectile; Position = a.Position; Rotation = a.Rotation; Size = Vector2(8.f,8.f); Texture = s.Texture; BodyType = Dynamic(shotBody); IsAlive = true}
                                newActors := shot :: !newActors
                                t
                            else 
                                s.LastShot

                        {a with ActorType = Player({s with IsShooting = false; LastShot = shotTime})}
                    else a
                | _ -> a
            )
        
        //create shots
        actors <- !newActors @ actors

           
        //Pos, Rot
        actors <- actors
            |> List.map (fun a ->
                match a.BodyType with
                | Dynamic(d) -> 

                    let v = Vector2(d.Velocity.X * float32 (1.0 - d.Friction) ,d.Velocity.Y * float32 (1.0 - d.Friction)) + d.Acceleration
                    let rv = d.RotationVelocity * (1.0 - d.RotationFriction) + d.RotationAcceleration

                    let p = Vector2(a.Position.X + v.X * dt, a.Position.Y + v.Y * dt)
                    let r = a.Rotation + (rv * float dt)

                    let db = {d with Velocity = v; RotationVelocity = rv; Acceleration = Vector2.Zero; RotationAcceleration = 0.0}
                    {a with Position = p; Rotation = r; BodyType = Dynamic(db)}    
                | Static -> a
            )
        ()
        
    override game.Draw (gameTime) =
        game.GraphicsDevice.Clear Color.White
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied)
        let draw actor =
            let x, y, w, h, r = (int actor.Position.X), (int actor.Position.Y), (int actor.Texture.Width), (int actor.Texture.Height), (float32 actor.Rotation)
            spriteBatch.Draw(actor.Texture, Rectangle(x, y, w, h), System.Nullable(), Color.White, r, Vector2(float32 (w / 2), float32 (h / 2)), SpriteEffects.None, float32 1)

        actors |> List.map (fun a -> draw a) |> ignore

        do spriteBatch.End ()
        ()