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

# For properties refer to https://github.com/Albacore/albacore/blob/master/lib/albacore/nuget_model.rb at Metadata class
desc "Pack the standard Zip library"
nugets_pack 'create_nuget_net20' => ['build/pkg', :versioning, :build] do |p|
  p.id                 = 'DotNetZip'
  p.version            = ENV['NUGET_VERSION']
  p.authors            = 'Henrik/Dino Chisa'
  p.description        = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
  p.summary            = 'A library for dealing with zip, bzip and zlib from .Net'
  p.language           = 'en-GB'
  p.project_url        = "https://github.com/haf/DotNetZip.Semverd"
  p.license_url        = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
  p.copyright          = 'Dino Chiesa'
  p.release_notes      = "Full version: #{ENV['BUILD_VERSION']}."

  #p.target_framework   = 'net20'
  p.configuration      = 'Release'
  p.files              = FileList['src/Zip/*.csproj']
  p.output             = 'build/pkg'
  p.exe                = 'buildsupport/NuGet.exe'
end

#desc "Pack the Android library"
#nugets_pack 'create_nuget_MonoAndroid10' => ['build/pkg', :versioning, :build] do |p|
#  p.configuration = 'Release'
#  p.files         = FileList['src/Zip.Android/*.csproj']
#  p.out           = 'build/pkg'
#  p.exe           = 'buildsupport/NuGet.exe'
#  
#  p.with_metadata do |m|
#	m.target_framework = 'MonoAndroid10'
#    # Don't override id, let the assembly name from the project files provide this.
#    m.id            = 'DotNetZip.Android'
#    m.version       = ENV['NUGET_VERSION']
#    # of the nuget at least
#    m.authors       = 'Henrik/Dino Chisa'
#    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
#    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
#    m.language      = 'en-GB'
#    m.copyright     = 'Dino Chiesa'
#    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
#    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
#    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
#  end
#  
#  p.no_project_dependencies
#end
#
#desc "Pack the iOS library"
#nugets_pack 'create_nuget_Xamarin.iOS10' => ['build/pkg', :versioning, :build] do |p|
#  p.configuration = 'Release'
#  p.files         = FileList['src/Zip.iOS/*.csproj']
#  p.out           = 'build/pkg'
#  p.exe           = 'buildsupport/NuGet.exe'
#  
#  p.with_metadata do |m|
#	m.target_framework = 'Xamarin.iOS10'
#    m.id            = 'DotNetZip.iOS'
#    m.version       = ENV['NUGET_VERSION']
#    # of the nuget at least
#    m.authors       = 'Henrik/Dino Chisa'
#    m.description   = 'A fork of the DotNetZip project without signing with a solution that compiles cleanly. This project aims to follow semver to avoid versioning conflicts. DotNetZip is a FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.'
#    m.summary       = 'A library for dealing with zip, bzip and zlib from .Net'
#    m.language      = 'en-GB'
#    m.copyright     = 'Dino Chiesa'
#    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
#    m.license_url   = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE"
#    m.project_url   = "https://github.com/haf/DotNetZip.Semverd"
#  end
#  
#  p.no_project_dependencies
#end

task :default do
	Rake::Task["create_nuget_net20"].invoke
#  %w|net20 MonoAndroid10 Xamarin.iOS10|.each do |fw|
#	Rake::Task["create_nuget_#{fw}"].invoke
#  end
end

