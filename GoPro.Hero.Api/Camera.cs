﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoPro.Hero.Api.Commands;
using GoPro.Hero.Api.Commands.CameraCommands;
using GoPro.Hero.Api.Exceptions;
using GoPro.Hero.Api.Utilities;

namespace GoPro.Hero.Api
{
    public class Camera:ICamera
    {
        protected Bacpac bacpac;

        private CameraInformation _information;
        private CameraExtendedSettings _extendedSettings;
        private CameraSettings _settings;

        public CameraInformation Information
        {
            get
            {
                this.GetInformation();
                return _information;
            }
        }
        public CameraExtendedSettings ExtendedSettings
        {
            get
            {
                this.GetExtendedSettings();
                return _extendedSettings;
            }
        }
        public CameraSettings Settings
        {
            get
            {
                this.GetSettings();
                return _settings;
            }
        }
        public BacpacStatus BacpacStatus
        {
            get { return this.bacpac.Status; }
        }
        public BacpacInformation BacpacInformation
        {
            get { return this.bacpac.Information; }
        }

        public string GetName()
        {
            var request = this.PrepareCommand<CommandCameraGetName>();
            var response = request.Send();

            var raw = response.RawResponse;
            var length = response.RawResponse[1];
            var name = Encoding.UTF8.GetString(raw, 2, length);
            if (!string.IsNullOrEmpty(name)) return name;
            name = this.Information.Name;
            return name.Fix();
        }
        public ICamera GetName(out string name)
        {
            name = this.GetName();

            return this;
        }
        public ICamera SetName(string name)
        {
            name = name.UrlEncode();

            var request = this.PrepareCommand<CommandCameraSetName>();
            request.Name = name;

            request.Send();

            return this;
        }

        private void GetInformation()
        {
            var request = this.PrepareCommand<CommandCameraInformation>();
            var response = request.Send();

            var stream = response.GetResponseStream();
            this._information.Update(stream);
        }
        private void GetSettings()
        {
            var request = this.PrepareCommand<CommandCameraSettings>();
            var response = request.Send();

            var stream = response.GetResponseStream();
            this._settings.Update(stream);
        }
        private void GetExtendedSettings()
        {
            var request = this.PrepareCommand<CommandCameraExtendedSettings>();
            var response = request.Send();

            var stream = response.GetResponseStream();
            _extendedSettings.Update(stream);
        }

        public ICamera Shutter(bool open)
        {
            bacpac.Shutter(open);
            return this;
        }
        public ICamera Power(bool on)
        {
            bacpac.Power(on);
            return this;
        }

        public ICamera Command(CommandRequest<ICamera> command)
        {
            var response = command.Send();
            return this;
        }
        public ICamera Command(CommandRequest<ICamera> command,out CommandResponse commandResponse,bool checkStatus=true)
        {
            commandResponse = this.Command(command,checkStatus);
            return this;
        }
        public CommandResponse Command(CommandRequest<ICamera> command, bool checkStatus = true)
        {
            return command.Send(checkStatus);
        }

        public T PrepareCommand<T>() where T : CommandRequest<ICamera>
        {
            return CommandRequest<ICamera>.Create<T>(this,this.bacpac.Address, passPhrase: this.bacpac.Password);
        }
        public ICamera PrepareCommand<T>(out T command) where T : CommandRequest<ICamera>
        {
            command = this.PrepareCommand<T>();
            return this;
        }

        public Camera(Bacpac bacpac)
        {
            _information = new CameraInformation();
            _extendedSettings = new CameraExtendedSettings();
            _settings = new CameraSettings();

            this.bacpac = bacpac;
        }

        public static T Create<T>(Bacpac bacpac) where T : Camera,ICamera
        {
            var camera = Activator.CreateInstance(typeof(T), bacpac) as T;
            return camera;
        }

        public static T Create<T>(string address) where T : Camera, ICamera
        {
            var bacpac = Bacpac.Create(address);
            var camera = Create<T>(bacpac);

            return camera;
        }
    }
}
