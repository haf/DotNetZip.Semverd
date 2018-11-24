require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Albacore::Tasks::Versionizer.new :versioning

task :restore do
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

paket = 'buildsupport/paket.exe'
file paket do
  dir = File.dirname(paket)
  sh "dotnet tool install Paket --version 5.190.0 --tool-path #{dir}"
end

#I'm sorry

desc "Pack the standard Zip library"
nugets_pack 'create_nuget_netfx' => ['build/pkg', :versioning, :build, paket] do |p|
  p.configuration = 'Release'
  p.files         = FileList['src/Zip/*.csproj']
  p.output        = 'build/pkg'
  p.exe           = paket

  p.metadata.instance_eval do |m|
    m.version       = ENV['NUGET_VERSION']
    # of the nuget at least
    m.authors       = 'Henrik/Dino Chisa'
    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
    m.language      = 'en-GB'
    m.copyright     = 'Dino Chiesa'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
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
    m.authors       = 'Henrik/Dino Chisa'
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
    m.authors       = 'Henrik/Dino Chisa'
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
  %w|netfx MonoAndroid10 Xamarin.iOS10|.each do |fw|
    Rake::Task["create_nuget_#{fw}"].invoke
  end
end

