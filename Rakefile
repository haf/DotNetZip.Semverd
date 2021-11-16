require 'bundler/setup'

require 'albacore'
require 'albacore/nuget_model'
require 'albacore/project'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Albacore::Tasks::Versionizer.new :versioning

paket = '.paket/paket.exe'
file paket do
  dir = File.dirname(paket)
  sh "dotnet tool install Paket --version 5.252.0 --tool-path #{dir}"
end

task :restore => [paket] do
   sh 'msbuild /t:restore src/DotNetZip.sln'
end

desc "Perform full build"
task :build => [:versioning, :restore, :asmver, :build_quick]

desc 'generate SolutionVersion.cs'
asmver :asmver do |a|
  ver = ENV['FORMAL_VERSION']
  a.file_path  = 'src/SolutionInfo.cs'
  a.namespace  = '' # empty for C# projects
  a.attributes \
    assembly_version: ver,
    assembly_file_version: ver,
    assembly_informational_version: ENV['BUILD_VERSION']
end

build :build_quick do |b|
  b.file = 'src/DotNetZip.sln'
  b.prop 'Configuration', 'Release'
end

directory 'build/pkg'

#I'm sorry
#Yeah? Well I'm even more sorry.

# This class is a glorious hack.  It adds the netfx assembly to the
# netstandard NuGet package.  The explanation of why is later.
#
# We also add .pdbs, for easier debugging.
class Albacore::NugetModel::Package
  singleton_class.send :alias_method, :orig_from_xxproj, :from_xxproj
  def self.from_xxproj proj, **opts
    package = self.orig_from_xxproj proj, opts

    netstandard_identifier = "netstandard2.0"
    netstandard_target_dir = "lib/#{netstandard_identifier}"
    # Use File.split because the Windows native directory separator (\)
    # is different from Ruby and Unix's (/).
    is_netstandard_pkg = (File.split(package.files.first&.target) == File.split(netstandard_target_dir))
    if is_netstandard_pkg
      package.add_file "bin/Release/#{netstandard_identifier}/DotNetZip.pdb", netstandard_target_dir

      netfx_identifier = "net40"
      netfx_target_dir = "lib/#{netfx_identifier}/"
      package.add_file "../Zip/bin/Release/DotNetZip.dll", netfx_target_dir
      package.add_file "../Zip/bin/Release/DotNetZip.pdb", netfx_target_dir
      package.add_file "../Zip/bin/Release/DotNetZip.xml", netfx_target_dir
    end

    package
  end

  alias_method :orig_to_template, :to_template
  # The .nuspec (generated from the NetStandard project) will say that
  # the package targets netstandard.  Because of this, we should also
  # explicitly say, again in the .nuspec, that the .NET Framework is
  # targetted.
  #
  # However, we cannot simply call package.add_dependency above,
  # because the .NET Framework package has no dependencies.  So instead
  # we override to_template.
  def to_template
    template = self.orig_to_template
    if @metadata.id == 'DotNetZip'
	  template = template.map { |s| s.sub '~>', '>=' }
      before = template.take_while { |l| l != 'dependencies' }
      after = template.drop_while { |l| l != 'dependencies' }.drop(1)
      template = [
        *before,
        'dependencies',
        '  framework: net40',
        *after,
      ]
    end
    template
  end
end

# We publish the .NET Framework and the .NET Standard libraries
# together, in the same NuGet package.  This makes the .NET Standard
# library more discoverable on NuGet.org, and prevents myriad packages.
desc "Pack the standard Zip library"
nugets_pack 'create_nuget_netfx_netstandard' => ['build/pkg', :versioning, :build, paket] do |p|
  p.configuration = 'Release'
  # The order of the .csproj files here makes a difference.  Albacore
  # reasonably assumes that each project creates a distinct NuGet
  # package.  But that isn't true for us.
  #
  # Putting the .NET Standard project last means the resulting NuGet
  # package will be based off the .NET Standard library.  This means
  # the package will contain the .NET Standard library, and the .nuspec
  # will correctly list said library's dependencies.  On the other
  # hand, this means that above we have to patch in the .NET Framework
  # library and information on its dependencies.
  p.files         = FileList[
                      'src/Zip/*.csproj',
                      'src/Zip NetStandard/*.csproj',
                    ]
  p.output        = 'build/pkg'
  p.exe           = paket

  p.metadata.instance_eval do |m|
    m.version       = ENV['NUGET_VERSION']
    # of the nuget at least
    m.authors       = 'Henrik/Dino Chiesa'
    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
    m.language      = 'en-GB'
    m.copyright     = 'Dino Chiesa'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
  end
end

# We need to override Albacore for Xamarin.  We need the correct lib
# subdirectories to be created, and for these to contain the right
# output files.
class Albacore::Project
  alias_method :original_target_framework, :target_framework
  # Used for lib directory:
  def target_framework
    if id == 'DotNetZip.Android' || id == 'DotNetZip.iOS'
      read_property('TargetFramework')
    else
      original_target_framework
    end
  end
  
  alias_method :original_try_outputs, :try_outputs
  def try_outputs conf, fw
    # For iOS, a subdir to the Release build directory is created.  Not
    # true for Android, so only Android is handled here.
    if id == 'DotNetZip.Android'
      outputs = []
      outputs << Albacore::OutputArtifact.new("bin/#{conf}/#{asmname}#{output_file_ext}", output_type)
      outputs
    else
      original_try_outputs(conf, fw)
    end
  end
end

desc "Pack the Android library"
nugets_pack 'create_nuget_MonoAndroid10' => ['build/pkg', :versioning, :build, paket] do |p|
  p.configuration = 'Release'
  p.files         = FileList['src/Zip.Android/*.csproj']
  p.output        = 'build/pkg'
  p.exe           = paket

  p.metadata.instance_eval do |m|
    m.version       = ENV['NUGET_VERSION']
    # of the nuget at least
    m.authors       = 'Henrik/Dino Chiesa'
    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
    m.language      = 'en-GB'
    m.copyright     = 'Dino Chiesa'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
  end
end

desc "Pack the iOS library"
nugets_pack 'create_nuget_Xamarin.iOS10' => ['build/pkg', :versioning, :build, paket] do |p|
  p.configuration = 'Release'
  p.files         = FileList['src/Zip.iOS/*.csproj']
  p.output        = 'build/pkg'
  p.exe           = paket
  
  p.metadata.instance_eval do |m|
    m.version       = ENV['NUGET_VERSION']
    # of the nuget at least
    m.authors       = 'Henrik/Dino Chiesa'
    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
    m.language      = 'en-GB'
    m.copyright     = 'Dino Chiesa'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
  end
end

task :default do
  %w|netfx_netstandard MonoAndroid10 Xamarin.iOS10|.each do |fw|
    Rake::Task["create_nuget_#{fw}"].invoke
  end
end

