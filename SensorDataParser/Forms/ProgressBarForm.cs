using Microsoft.Extensions.Configuration;
using SensorDataParser.Models;
using SensorDataParser.Parser;
using System.Data;

namespace SensorDataParser
{
    public partial class ProgressBarForm : Form
    {
        private readonly string[] _filenames;
        private readonly int _count;
        private readonly IConfigurationRoot _configuration;

        private delegate void UpdateLablelDelegate(string value);

        public ProgressBarForm(string[] filenames, int count, IConfigurationRoot configuration)
        {
            InitializeComponent();
            _filenames = filenames;
            _count = count;
            _configuration = configuration;
        }

        private void StepProgress()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(StepProgress));
            }
            else
            {
                progressBar.PerformStep();
            }
        }

        private void XMLFileCultivation(string root)
        {
            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;

            var findService = new XmlEntityFindService();

            var unitRefTypes = findService.FindAll<UnitRefType>(root);
            var unitRefTypesRepo = new GenericRepository<UnitRefType>(_configuration);
            unitRefTypes = unitRefTypesRepo.InsertOrGetExisting(unitRefTypes);
            StepProgress();

            var unitEngTypes = findService.FindAll<UnitEngType>(root);
            UpdateRelatedEntities(
                unitRefTypes,
                unitEngTypes,
                item => item.URTID,
                item1 => item1.URTID,
                (item, item1) => item1.SURTID = item.Id
            );
            var unitEngTypesRepo = new GenericRepository<UnitEngType>(_configuration);
            unitEngTypes = unitEngTypesRepo.InsertOrGetExisting(unitEngTypes);
            StepProgress();

            var biasVoltages = findService.FindAll<BiasVoltage>(root);
            var biasVoltagesRepo = new GenericRepository<BiasVoltage>(_configuration);
            biasVoltages = biasVoltagesRepo.InsertOrGetExisting(biasVoltages);
            StepProgress();

            var paramTypes = findService.FindAll<ParamType>(root);
            var paramTypesRepo = new GenericRepository<ParamType>(_configuration);
            paramTypes = paramTypesRepo.InsertOrGetExisting(paramTypes);
            StepProgress();

            var axisTypes = findService.FindAll<AxisType>(root);
            var axisTypesRepo = new GenericRepository<AxisType>(_configuration);
            axisTypes = axisTypesRepo.InsertOrGetExisting(axisTypes);
            StepProgress();

            var folders = findService.FindAll<Folder>(root);
            var foldersRepo = new GenericRepository<Folder>(_configuration);
            folders = foldersRepo.InsertOrGetExisting(folders);
            StepProgress();

            var machines = findService.FindAll<Machine>(root);
            UpdateRelatedEntities(
                folders,
                machines,
                item => item.FolderID,
                item1 => item1.FolderID,
                (item, item1) => item1.SFolderID = item.Id
            );
            var machinesRepo = new GenericRepository<Machine>(_configuration);
            machines = machinesRepo.InsertOrGetExisting(machines);
            StepProgress();

            var sensors = findService.FindAll<Sensor>(root);
            UpdateRelatedEntities(
                biasVoltages,
                sensors,
                item => item.BiasVoltageID,
                item1 => item1.BiasVoltageID,
                (item, item1) => item1.SBiasVoltageID = item.Id
            );
            var sensorsRepo = new GenericRepository<Sensor>(_configuration);
            sensors = sensorsRepo.InsertOrGetExisting(sensors);
            StepProgress();

            var points = findService.FindAll<Models.Point>(root);
            UpdateRelatedEntities(
                machines,
                points,
                item => item.MachineID,
                item1 => item1.MachineID,
                (item, item1) => item1.SMachineID = item.Id
            );
            var pointsRepo = new GenericRepository<Models.Point>(_configuration);
            points = pointsRepo.InsertOrGetExisting(points);
            StepProgress();

            var paramSets = findService.FindAll<ParamSet>(root);
            UpdateRelatedEntities(
                paramTypes,
                paramSets,
                item => item.ParamTypeID,
                item1 => item1.ParamTypeID,
                (item, item1) => item1.SParamTypeID = item.Id
            );
            UpdateRelatedEntities(
                unitRefTypes,
                paramSets,
                item => item.URTID,
                item1 => item1.MeasurementURTID,
                (item, item1) => item1.SMeasurementURTID = item.Id
            );
            UpdateRelatedEntities(
                unitEngTypes,
                paramSets,
                item => item.UETID,
                item1 => item1.KeypadUETID,
                (item, item1) => item1.SKeypadUETID = item.Id
            );
            var paramSetsRepo = new GenericRepository<ParamSet>(_configuration);
            paramSets = paramSetsRepo.InsertOrGetExisting(paramSets);
            StepProgress();

            var axes = findService.FindAll<Axis>(root);
            UpdateRelatedEntities(
                points,
                axes,
                item => item.PointID,
                item1 => item1.PointID,
                (item, item1) => item1.SPointID = item.Id
            );
            UpdateRelatedEntities(
                axisTypes,
                axes,
                item => item.AxisTypeID,
                item1 => item1.AxisTypeID,
                (item, item1) => item1.SAxisTypeID = item.Id
            );
            var axesRepo = new GenericRepository<Axis>(_configuration);
            axes = axesRepo.InsertOrGetExisting(axes);
            StepProgress();

            var multiSensors = findService.FindAll<MultiSensor>(root);
            UpdateRelatedEntities(
                sensors,
                multiSensors,
                item => item.SensorID,
                item1 => item1.SensorID1,
                (item, item1) => item1.SSensorID1 = item.Id
            );
            UpdateRelatedEntities(
                sensors,
                multiSensors,
                item => item.SensorID,
                item1 => item1.SensorID2,
                (item, item1) => item1.SSensorID2 = item.Id
            );
            UpdateRelatedEntities(
                sensors,
                multiSensors,
                item => item.SensorID,
                item1 => item1.SensorID3,
                (item, item1) => item1.SSensorID3 = item.Id
            );
            UpdateRelatedEntities(
                sensors,
                multiSensors,
                item => item.SensorID,
                item1 => item1.SensorID4,
                (item, item1) => item1.SSensorID4 = item.Id
            );
            var multiSensorsRepo = new GenericRepository<MultiSensor>(_configuration);
            multiSensors = multiSensorsRepo.InsertOrGetExisting(multiSensors);
            StepProgress();

            var notes = findService.FindAll<Note>(root);
            var notesRepo = new GenericRepository<Note>(_configuration);
            notes = notesRepo.InsertOrGetExisting(notes);
            StepProgress();

            var schedules = findService.FindAll<Schedule>(root);
            var schedulesRepo = new GenericRepository<Schedule>(_configuration);
            UpdateRelatedEntities(
               multiSensors,
               schedules,
               item => item.MultiSensorID,
               item1 => item1.MultiSensorID,
               (item, item1) => item1.SMultiSensorID = item.Id
            );
            UpdateRelatedEntities(
                paramSets,
                schedules,
                item => item.ParamSetID,
                item1 => item1.ParamSetID,
                (item, item1) => item1.SParamSetID = item.Id
            );
            UpdateRelatedEntities(
                axes,
                schedules,
                item => item.AxisID,
                item1 => item1.AxisID,
                (item, item1) => item1.SAxisID = item.Id
            );
            schedules = schedulesRepo.InsertOrGetExisting(schedules);
            StepProgress();

            var records = findService.FindAll<Record>(root);
            var recordsRepo = new GenericRepository<Record>(_configuration);
            UpdateRelatedEntities(
                schedules,
                records,
                item => item.ScheduleID,
                item1 => item1.ScheduleID,
                (item, item1) => item1.SScheduleID = item.Id
            );
            UpdateRelatedEntities(
                paramTypes,
                records,
                item => item.ParamTypeID,
                item1 => item1.ParamTypeID,
                (item, item1) => item1.SParamTypeID = item.Id
            );
            UpdateRelatedEntities(
                unitEngTypes,
                records,
                item => item.UETID,
                item1 => item1.NativeUETID,
                (item, item1) => item1.SNativeUETID = item.Id
            );
            records = recordsRepo.InsertOrGetExisting(records);
            StepProgress();
        }

        private async void ProgressBarFormLoad(object sender, EventArgs e)
        {
            try
            {
                progressBar.Maximum = _filenames.Length * _count;

                await Task.Run(() =>
                {
                    foreach (var root in _filenames)
                    {
                        Invoke(new UpdateLablelDelegate(UpdateLabelValue), root);
                        XMLFileCultivation(root);
                    }
                });

                MessageBox.Show("Файлы успешно загружены", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Close();
            }
        }

        private static void UpdateRelatedEntities<TKey, TSource, TTarget>(
            IEnumerable<TSource> sourceCollection,
            IEnumerable<TTarget> targetCollection,
            Func<TSource, TKey> sourceKeySelector,
            Func<TTarget, TKey> targetKeySelector,
            Action<TSource, TTarget> updateAction)
        {
            foreach (var (source, target) in from source in sourceCollection
                                             from target in targetCollection
                                             where sourceKeySelector(source)!.Equals(targetKeySelector(target))
                                             select (source, target))
            {
                updateAction(source, target);
            }
        }

        private void UpdateLabelValue(string value)
        {
            label2.Text = value;
        }
    }
}