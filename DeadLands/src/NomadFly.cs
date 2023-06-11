﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Deadlands
{
    internal class NomadFly
    { 
        public static float speed;
        public static bool LimitSpeed = true;

        public static ConditionalWeakTable<Player, NomadEX> SlideData = new();

        public static void OnInit()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_UpdateMSC;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            SlideData.Add(self, new NomadEX(self));
        }

        private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            //Fly values
            if (!SlideData.TryGetValue(self, out var player) || !player.isNomad)
            {
                return;
            }

            //Fly constants
            const float normalGravity = 0.9f;
            const float normalAirFriction = 0.999f;
            const float flightGravity = 0.22f;
            const float flightAirFriction = 0.8f;
            const float flightKickinDuration = 6f;

            //if guh'uh fly
            if (!self.dead)
            {
                if (player.CanSlide)
                {
                    //Don't fly if is grabbing a corner?
                    if (self.animation == Player.AnimationIndex.HangFromBeam)
                    {
                        player.preventSlide = 15;
                    }
                    else if (player.preventSlide > 0)
                    {
                        player.preventSlide--;
                    }

                    //True
                    if (!player.isSliding)
                    {
                        speed = 2f;
                        LimitSpeed = true;
                    }

                    //JumperDumper is the check for the Flap before the fly
                    if (player.isSliding)
                    {
                        //The wind sound (So smooth)
                        player.windSound.Volume = Mathf.Lerp(0f, 0.4f, player.slideDuration / flightKickinDuration);

                        //Slide duration must be removed in the nexts versions
                        player.slideDuration++;
                        //Start Shaking before a couple of seconds flying
                        self.AerobicIncrease(0.08f);

                        //Fly gravity and Air fiction
                        self.gravity = Mathf.Lerp(normalGravity, flightGravity, player.slideDuration / flightKickinDuration);
                        self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, player.slideDuration / flightKickinDuration);

                        //Limit speed It's a check to make the flight faster and then, if it reaches a maximum speed, start descending
                        if (LimitSpeed)
                        {
                            //The custom speed that increases
                            speed = RWCustom.Custom.LerpAndTick(speed, 7f, 0.01f, 0.3f);

                            //If the speed reach the speed limit, you receive a fine
                            if (speed >= 7f)
                            {
                                //Fine $50000 speed limit!
                                LimitSpeed = false;
                            }

                            //Horizontal speed positive
                            if (self.input[0].x > 0)
                            {
                                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + speed;
                                self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
                            }
                            //Horizontal speed negative
                            else if (self.input[0].x < 0)
                            {
                                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - speed;
                                self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
                            }

                            //Vertical speed positive in 0g
                            if (self.room.gravity <= 0.5)
                            {
                                if (self.input[0].y > 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 0.3f;
                                }
                                //Vertical speed negative
                                else if (self.input[0].y < 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.3f;
                                }
                            }

                            //Hacks
                            else if (player.UnlockedVerticalFlight)
                            {
                                if (self.input[0].y > 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + speed * 0.8f;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 0.6f;
                                }
                                //Vertical speed that gives the sensation of losing altitude negative
                                else if (self.input[0].y < 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.6f;
                                }
                            }
                        }

                        //If you get the fine, you start driving slower!
                        if (!LimitSpeed)
                        {
                            //Decresing speed
                            speed = RWCustom.Custom.LerpAndTick(speed, 0f, 0.005f, 0.003f);

                            //If speed 0, you pay the fine and you can drive faster!!
                            if (speed == 0f)
                            {
                                //No more fine :3
                                LimitSpeed = true;
                            }

                            //Horizontal speed positive in 0g
                            if (self.input[0].x > 0)
                            {
                                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + speed;
                                self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
                            }
                            else if (self.input[0].x < 0)
                            {
                                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - speed;
                                self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
                            }

                            //Vertical speed positive
                            if (self.room.gravity <= 0.5)
                            {
                                if (self.input[0].y > 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                                }
                                else if (self.input[0].y < 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                                }
                            }
                            //Vertical speed that gives the sensation of losing altitude positive
                            else if (player.UnlockedVerticalFlight)
                            {
                                //Positive
                                if (self.input[0].y > 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + speed * 1f;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                                }
                                //Negative
                                else if (self.input[0].y < 0)
                                {
                                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - speed;
                                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                                }
                            }

                            if (speed <= 1f)
                            {
                                player.StopSliding();
                            }
                        }

                        //Recovering stamina, this must be removed in the next verions
                        player.slideStaminaRecoveryCooldown = 40;
                        player.SlideStamina--;

                        //STOP THAT CAR!!
                        if (!self.input[0].jmp || !player.CanSustainFlight())
                        {
                            player.StopSliding();
                        }

                    }
                    else
                    {
                        //wind volume
                        player.windSound.Volume = Mathf.Lerp(1f, 0f, player.timeSinceLastSlide / flightKickinDuration);

                        //Counting the Slide time, this must be removed in the next versions
                        player.timeSinceLastSlide++;

                        //Volume to 0? :o
                        player.windSound.Volume = 0f;

                        //Recovering slide
                        if (player.slideStaminaRecoveryCooldown > 0)
                        {
                            player.slideStaminaRecoveryCooldown--;
                        }
                        //Stamina
                        else
                        {
                            player.SlideStamina = Mathf.Min(player.SlideStamina + player.SlideRecovery, player.SlideStaminaMax);
                        }

                        //Start driving!
                        if (self.wantToJump > 0 && player.SlideStamina > player.MinimumSlideStamina && player.CanSustainFlight())
                        {
                            player.InitiateSlide();
                        }

                        //Set the gravity and air friction!
                        self.airFriction = normalAirFriction;
                        self.gravity = normalGravity;
                    }
                }

                //DO NOT GRAB THAT
                if (player.preventGrabs > 0)
                {
                    player.preventGrabs--;
                }
            }
            //Sound update
            player.windSound.Update();
        }
    }
}