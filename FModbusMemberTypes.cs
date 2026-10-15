namespace MCF.Modbus
{
    /// <summary>
    /// The MCF.Modbus member type that encapsulates an <see cref="System.Int32">int</see> backed by a Holding Register.
    /// </summary>
    /// <remarks>
    /// When the application is in <see cref="EProgramState">Running</see> mode and 
    /// <see cref="FDeviceDouble.TimeBetweenUpdates">TimeBetweenUpdates</see> is greater than zero, 
    /// the <see cref="FDevice"/> object polls the Holding Register to keep the member value synchronized.
    /// In addition, when the application changes the member value, that value is written to the Holding Register.
    /// </remarks>
    /// <example>
    /// <code>
    ///     var TemperatureSet = new FModbusHoldingRegister(this, this, "TemperatureSet", 500, EMemberCategory.Command)
    ///     {
    ///         Description = "Heater temperature set-point.",
    ///     };
    /// </code>
    /// </example>
    /// <seealso cref="FModbusMaster"/>
    /// <seealso cref="FDeviceInteger"/>
    public class FModbusHoldingRegister : FDeviceInteger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FModbusHoldingRegister"/> class.
        /// </summary>
        /// <param name="Owner">The member's Machine View owner object.</param>
        /// <param name="Device">The member's <see cref="FModbusMaster"/> object.</param>
        /// <param name="Name">The member's name.</param>
        /// <param name="Address">The coil's address.</param>
        /// <param name="TimeBetweenUpdates">The update interval in msec, or zero for no updates.</param>
        /// <param name="Category">The member's catagory.</param>
        /// <remarks>
        /// Owner and Device will often be the same, but can be different if the member should be owned by 
        /// a different object than the controller.
        /// </remarks>
        public FModbusHoldingRegister(IMemberCollection Owner, FDevice Device, string Name, int Address, int TimeBetweenUpdates, EMemberCategory Category)
            : base(Owner, Device, Name, string.Format("HoldingRegister{0}={{0}}", Address), string.Format("HoldingRegister{0}", Address), TimeBetweenUpdates, Category)
        {
            Type = "modbus holding register";
        }

        /// <summary>
        /// Obtains a reference to a <see cref="FModbusHoldingRegister"/> member by full name.
        /// </summary>
        /// <param name="fullName">Full name of the member.</param>
        /// <param name="caller">Optional caller object (for the member report).</param>
        /// <returns>A reference to the member.</returns>
        /// <remarks>
        /// <note type="caution">
        /// All object and member references should be obtained during construction and initialization.
        /// If this method fails to return the requested reference, the program will terminate.
        /// Reasons for failure can include spelling error, type mismatch, or member does not yet exist.
        /// </note>
        /// </remarks>
        /// <example>
        /// <code>
        ///     var DoorLock = FModbusHoldingRegister.GetRef("Plc.DoorLock", this);
        /// </code>
        /// </example>
        new public static FModbusHoldingRegister GetRef(string fullName, object caller = null)
        {
            FBase element;
            g.Verify(FBase.GetElement(fullName, out element, caller), "\"" + fullName + "\" (FModbusHoldingRegister) name not found.");
            g.Verify(element is FModbusHoldingRegister, "\"" + fullName + "\" is not an FModbusHoldingRegister.");
            g.Verify(!(element as FMember).IsMemberFlagSet(EMemberFlags.Private), "\"" + fullName + "\" (FModbusHoldingRegister) is private.");
            return (FModbusHoldingRegister)element;
        }
    }


    /// <summary>
    /// The MCF.Modbus member type that encapsulates an <see cref="System.Int32">int</see> backed by an Input Register.
    /// </summary>
    /// <remarks>
    /// When the application is in <see cref="EProgramState">Running</see> mode and 
    /// <see cref="FDeviceDouble.TimeBetweenUpdates">TimeBetweenUpdates</see> is greater than zero, 
    /// the <see cref="FDevice"/> object polls the Input Register to keep the member value synchronized.
    /// </remarks>
    /// <example>
    /// <code>
    ///     var PressPressure = new FModbusInputRegister(this, this, "PressPressure", 500)
    ///     {
    ///         Description = "Load cell output.",
    ///     };
    /// </code>
    /// </example>
    /// <seealso cref="FModbusMaster"/>
    /// <seealso cref="FDeviceInteger"/>
    public class FModbusInputRegister : FDeviceInteger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FModbusInputRegister"/> class.
        /// </summary>
        /// <param name="Owner">The member's Machine View owner object.</param>
        /// <param name="Device">The member's <see cref="FModbusMaster"/> object.</param>
        /// <param name="Name">The member's name.</param>
        /// <param name="Address">The input register's address.</param>
        /// <param name="TimeBetweenUpdates">The update interval in msec, or zero for no updates.</param>
        /// <remarks>
        /// Owner and Device will often be the same, but can be different if the member should be owned by 
        /// a different object than the controller.
        /// </remarks>
        public FModbusInputRegister(FObject Owner, FDevice Device, string Name, int Address, int TimeBetweenUpdates)
            : base(Owner, Device, Name, null, string.Format("InputRegister{0}", Address), TimeBetweenUpdates, EMemberCategory.Status)
        {
            Type = "modbus input register";
        }

        /// <summary>
        /// Obtains a reference to a <see cref="FModbusInputRegister"/> member by full name.
        /// </summary>
        /// <param name="fullName">Full name of the member.</param>
        /// <param name="caller">Optional caller object (for the member report).</param>
        /// <returns>A reference to the member.</returns>
        /// <remarks>
        /// <note type="caution">
        /// All object and member references should be obtained during construction and initialization.
        /// If this method fails to return the requested reference, the program will terminate.
        /// Reasons for failure can include spelling error, type mismatch, or member does not yet exist.
        /// </note>
        /// </remarks>
        /// <example>
        /// <code>
        ///     var LightCurtain = FModbusInputRegister.GetRef("Plc.LightCurtain", this);
        /// </code>
        /// </example>
        new public static FModbusInputRegister GetRef(string fullName, object caller = null)
        {
            FBase element;
            g.Verify(FBase.GetElement(fullName, out element, caller), "\"" + fullName + "\" (FModbusInputRegister) name not found.");
            g.Verify(element is FModbusInputRegister, "\"" + fullName + "\" is not an FModbusInputRegister.");
            g.Verify(!(element as FMember).IsMemberFlagSet(EMemberFlags.Private), "\"" + fullName + "\" (FModbusInputRegister) is private.");
            return (FModbusInputRegister)element;
        }
    }


    /// <summary>
    /// The MCF.Modbus member type that encapsulates a <see cref="System.Boolean">bool</see> backed by a Coil.
    /// </summary>
    /// <remarks>
    /// When the application is in <see cref="EProgramState">Running</see> mode and 
    /// <see cref="FDeviceDouble.TimeBetweenUpdates">TimeBetweenUpdates</see> is greater than zero, 
    /// the <see cref="FDevice"/> object polls the Coil to keep the member value synchronized.
    /// In addition, when the application changes the member value, that value is written to the Coil.
    /// </remarks>
    /// <example>
    /// <code>
    ///     var DoorLock = new FModbusCoil(this, this, "DoorLock", 500, EMemberCategory.Variable)
    ///     {
    ///         Description = "True when the door is locked.",
    ///     };
    /// </code>
    /// </example>
    /// <seealso cref="FModbusMaster"/>
    /// <seealso cref="FDeviceBoolean"/>
    public class FModbusCoil : FDeviceBoolean
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FModbusCoil"/> class.
        /// </summary>
        /// <param name="Owner">The member's Machine View owner object.</param>
        /// <param name="Device">The member's <see cref="FModbusMaster"/> object.</param>
        /// <param name="Name">The member's name.</param>
        /// <param name="Address">The coil's address.</param>
        /// <param name="TimeBetweenUpdates">The update interval in msec, or zero for no updates.</param>
        /// <param name="Category">The member's catagory.</param>
        /// <remarks>
        /// Owner and Device will often be the same, but can be different if the member should be owned by 
        /// a different object than the controller.
        /// </remarks>
        public FModbusCoil(IMemberCollection Owner, FDevice Device, string Name, int Address, int TimeBetweenUpdates, EMemberCategory Category)
            : base(Owner, Device, Name, string.Format("Coil{0}={{0}}", Address), string.Format("Coil{0}", Address), TimeBetweenUpdates, Category)
        {
            Type = "modbus coil";
        }

        /// <summary>
        /// Obtains a reference to a <see cref="FModbusCoil"/> member by full name.
        /// </summary>
        /// <param name="fullName">Full name of the member.</param>
        /// <param name="caller">Optional caller object (for the member report).</param>
        /// <returns>A reference to the member.</returns>
        /// <remarks>
        /// <note type="caution">
        /// All object and member references should be obtained during construction and initialization.
        /// If this method fails to return the requested reference, the program will terminate.
        /// Reasons for failure can include spelling error, type mismatch, or member does not yet exist.
        /// </note>
        /// </remarks>
        /// <example>
        /// <code>
        ///     var DoorLock = FModbusCoil.GetRef("Plc.DoorLock", this);
        /// </code>
        /// </example>
        new public static FModbusCoil GetRef(string fullName, object caller = null)
        {
            FBase element;
            g.Verify(FBase.GetElement(fullName, out element, caller), "\"" + fullName + "\" (FModbusCoil) name not found.");
            g.Verify(element is FModbusCoil, "\"" + fullName + "\" is not an FModbusCoil.");
            g.Verify(!(element as FMember).IsMemberFlagSet(EMemberFlags.Private), "\"" + fullName + "\" (FModbusCoil) is private.");
            return (FModbusCoil)element;
        }
    }


    /// <summary>
    /// The MCF.Modbus member type that encapsulates a <see cref="System.Boolean">bool</see> backed by a Discrete Input.
    /// </summary>
    /// <remarks>
    /// When the application is in <see cref="EProgramState">Running</see> mode and 
    /// <see cref="FDeviceDouble.TimeBetweenUpdates">TimeBetweenUpdates</see> is greater than zero, 
    /// the <see cref="FDevice"/> object polls the Discrete Input to keep the member value synchronized.
    /// </remarks>
    /// <example>
    /// <code>
    ///     var LightCurtain = new FModbusDiscreteInput(this, this, "LightCurtain", 500)
    ///     {
    ///         Description = "True when the light curtain is activated.",
    ///     };
    /// </code>
    /// </example>
    /// <seealso cref="FModbusMaster"/>
    /// <seealso cref="FDeviceBoolean"/>
    public class FModbusDiscreteInput : FDeviceBoolean
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FModbusDiscreteInput"/> class.
        /// </summary>
        /// <param name="Owner">The member's Machine View owner object.</param>
        /// <param name="Device">The member's <see cref="FModbusMaster"/> object.</param>
        /// <param name="Name">The member's name.</param>
        /// <param name="Address">The discrete input's address.</param>
        /// <param name="TimeBetweenUpdates">The update interval in msec, or zero for no updates.</param>
        /// <remarks>
        /// Owner and Device will often be the same, but can be different if the member should be owned by 
        /// a different object than the controller.
        /// </remarks>
        public FModbusDiscreteInput(FObject Owner, FDevice Device, string Name, int Address, int TimeBetweenUpdates)
            : base(Owner, Device, Name, null, string.Format("DiscreteInput{0}", Address), TimeBetweenUpdates, EMemberCategory.Status)
        {
            Type = "modbus discrete input";
        }

        /// <summary>
        /// Obtains a reference to a <see cref="FModbusDiscreteInput"/> member by full name.
        /// </summary>
        /// <param name="fullName">Full name of the member.</param>
        /// <param name="caller">Optional caller object (for the member report).</param>
        /// <returns>A reference to the member.</returns>
        /// <remarks>
        /// <note type="caution">
        /// All object and member references should be obtained during construction and initialization.
        /// If this method fails to return the requested reference, the program will terminate.
        /// Reasons for failure can include spelling error, type mismatch, or member does not yet exist.
        /// </note>
        /// </remarks>
        /// <example>
        /// <code>
        ///     var LightCurtain = FModbusDiscreteInput.GetRef("Plc.LightCurtain", this);
        /// </code>
        /// </example>
        new public static FModbusDiscreteInput GetRef(string fullName, object caller = null)
        {
            FBase element;
            g.Verify(FBase.GetElement(fullName, out element, caller), "\"" + fullName + "\" (FModbusDiscreteInput) name not found.");
            g.Verify(element is FModbusDiscreteInput, "\"" + fullName + "\" is not an FModbusDiscreteInput.");
            g.Verify(!(element as FMember).IsMemberFlagSet(EMemberFlags.Private), "\"" + fullName + "\" (FModbusDiscreteInput) is private.");
            return (FModbusDiscreteInput)element;
        }
    }
}
